using BeerCompetition.Competition.Application.Features.RegisterOrganizer;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.IntegrationTests.Infrastructure;
using BeerCompetition.Shared.Kernel;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BeerCompetition.Competition.IntegrationTests.Features.RegisterOrganizer;

/// <summary>
/// Integration tests for RegisterOrganizer feature.
/// Uses Testcontainers for real Postgres, WebApplicationFactory for API, and Respawn for cleanup.
/// 
/// Note: These tests use TestTenantProvider to set tenant context AFTER tenant creation,
/// avoiding the need for IgnoreQueryFilters() in most verifications.
/// </summary>
public class RegisterOrganizerIntegrationTests : IntegrationTestBase
{
    private readonly IMediator _mediator;

    public RegisterOrganizerIntegrationTests(IntegrationTestWebApplicationFactory factory) 
        : base(factory)
    {
        _mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_CompleteFlow_CreatesAllEntities()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "integration@test.com",
            Password: "SecurePass123",
            OrganizationName: "Integration Test Org",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        // Configure mock Keycloak service
        Factory.KeycloakService.CreateUserAsync(
                command.Email, 
                command.Password, 
                Arg.Any<bool>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        Factory.KeycloakService.AssignRoleAsync(
                keycloakUserId, 
                "organizer", 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        Factory.KeycloakService.SetUserAttributeAsync(
                keycloakUserId, 
                "tenant_id", 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(keycloakUserId);
        result.Value.TenantId.Should().NotBeEmpty();

        // Set tenant context for verification queries
        Factory.TenantProvider.SetTenant(result.Value.TenantId);

        // Verify tenant was created in database (use fresh context with tenant set)
        var verifyContext = GetFreshDbContext();
        var tenant = await verifyContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == result.Value.TenantId, CancellationToken.None);
        tenant.Should().NotBeNull();
        tenant!.OrganizationName.Should().Be(command.OrganizationName);
        tenant.Email.Should().Be(command.Email);

        // Verify Keycloak interactions
        await Factory.KeycloakService.Received(1).CreateUserAsync(
            command.Email, 
            command.Password, 
            Arg.Any<bool>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>());
        
        await Factory.KeycloakService.Received(1).AssignRoleAsync(
            keycloakUserId, 
            "organizer", 
            Arg.Any<CancellationToken>());
        
        await Factory.KeycloakService.Received(1).SetUserAttributeAsync(
            keycloakUserId,
            "tenant_id",
            result.Value.TenantId.ToString(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_DoesNotCreateTenant()
    {
        // Arrange - Create first tenant
        var firstCommand = new RegisterOrganizerCommand(
            Email: "duplicate@test.com",
            Password: "SecurePass123",
            OrganizationName: "First Org",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        Factory.KeycloakService.CreateUserAsync(
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<bool>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        Factory.KeycloakService.AssignRoleAsync(
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        Factory.KeycloakService.SetUserAttributeAsync(
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var firstResult = await _mediator.Send(firstCommand, CancellationToken.None);
        
        // Set tenant context to first tenant
        Factory.TenantProvider.SetTenant(firstResult.Value!.TenantId);

        // Reset mock to track only second call
        Factory.KeycloakService.ClearReceivedCalls();

        // Act - Try to create second tenant with same email
        var secondCommand = new RegisterOrganizerCommand(
            Email: "duplicate@test.com",
            Password: "AnotherPass456",
            OrganizationName: "Second Org",
            PlanName: "BASIC"
        );

        var result = await _mediator.Send(secondCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already");

        // Verify Keycloak was not called for second attempt
        await Factory.KeycloakService.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<bool>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>());

        // Verify only one tenant exists (should be the first one still set in context)
        var verifyContext = GetFreshDbContext();
        var tenant = await verifyContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Email == "duplicate@test.com", CancellationToken.None);
        
        tenant.Should().NotBeNull();
        tenant!.OrganizationName.Should().Be("First Org");
    }

    [Fact]
    public async Task Handle_KeycloakFailure_RollsBackTransaction()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "rollback@test.com",
            Password: "SecurePass123",
            OrganizationName: "Rollback Test Org",
            PlanName: "STANDARD"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        Factory.KeycloakService.CreateUserAsync(
                command.Email, 
                command.Password, 
                Arg.Any<bool>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        Factory.KeycloakService.AssignRoleAsync(
                keycloakUserId, 
                "organizer", 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Simulate failure on SetUserAttributeAsync (tenant_id)
        Factory.KeycloakService.SetUserAttributeAsync(
                keycloakUserId, 
                "tenant_id", 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Attribute update failed"));

        Factory.KeycloakService.DeleteUserAsync(
                keycloakUserId, 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Attribute update failed");

        // Verify tenant was NOT persisted (rollback)
        // Note: We use IgnoreQueryFilters here because no tenant was successfully created
        var verifyContext = GetFreshDbContext();
        var tenant = verifyContext.Set<Tenant>()
            .IgnoreQueryFilters()
            .FirstOrDefault(t => t.Email == command.Email);
        tenant.Should().BeNull();

        // Verify Keycloak cleanup was called
        await Factory.KeycloakService.Received(1).DeleteUserAsync(
            keycloakUserId, 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleOrganizers_IsolatesTenants()
    {
        // Arrange - Create two separate organizers
        var organizer1Command = new RegisterOrganizerCommand(
            Email: "organizer1@test.com",
            Password: "SecurePass123",
            OrganizationName: "Org 1",
            PlanName: "TRIAL"
        );

        var organizer2Command = new RegisterOrganizerCommand(
            Email: "organizer2@test.com",
            Password: "SecurePass456",
            OrganizationName: "Org 2",
            PlanName: "PRO"
        );

        // Mock Keycloak to return different user IDs
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();

        Factory.KeycloakService.CreateUserAsync(
                organizer1Command.Email, 
                Arg.Any<string>(), 
                Arg.Any<bool>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(userId1));

        Factory.KeycloakService.CreateUserAsync(
                organizer2Command.Email, 
                Arg.Any<string>(), 
                Arg.Any<bool>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(userId2));

        Factory.KeycloakService.AssignRoleAsync(
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        Factory.KeycloakService.SetUserAttributeAsync(
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result1 = await _mediator.Send(organizer1Command, CancellationToken.None);
        var result2 = await _mediator.Send(organizer2Command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        result1.Value!.TenantId.Should().NotBe(result2.Value!.TenantId);

        // Verify first tenant
        Factory.TenantProvider.SetTenant(result1.Value.TenantId);
        var verifyContext1 = GetFreshDbContext();
        var tenant1 = await verifyContext1.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == result1.Value.TenantId, CancellationToken.None);
        tenant1.Should().NotBeNull();
        tenant1!.Email.Should().Be("organizer1@test.com");

        // Verify second tenant
        Factory.TenantProvider.SetTenant(result2.Value.TenantId);
        var verifyContext2 = GetFreshDbContext();
        var tenant2 = await verifyContext2.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == result2.Value.TenantId, CancellationToken.None);
        tenant2.Should().NotBeNull();
        tenant2!.Email.Should().Be("organizer2@test.com");
    }
}
