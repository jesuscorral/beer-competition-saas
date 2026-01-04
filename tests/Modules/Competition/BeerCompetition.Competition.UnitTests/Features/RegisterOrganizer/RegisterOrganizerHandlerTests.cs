using BeerCompetition.Competition.Application.Features.RegisterOrganizer;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Infrastructure.ExternalServices;
using BeerCompetition.Shared.Kernel;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeerCompetition.Competition.UnitTests.Features.RegisterOrganizer;

public class RegisterOrganizerHandlerTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<RegisterOrganizerHandler> _logger;
    private readonly RegisterOrganizerHandler _handler;

    public RegisterOrganizerHandlerTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _competitionRepository = Substitute.For<ICompetitionRepository>();
        _keycloakService = Substitute.For<IKeycloakService>();
        _logger = Substitute.For<ILogger<RegisterOrganizerHandler>>();

        _handler = new RegisterOrganizerHandler(
            _tenantRepository,
            _competitionRepository,
            _keycloakService,
            _logger
        );
    }

    [Fact]
    public async Task Handle_NewOrganizer_CreatesUserTenantAndCompetition()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(null));

        _keycloakService.CreateUserAsync(command.Email, command.Password, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        _keycloakService.AssignRoleAsync(keycloakUserId, "organizer", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _tenantRepository.AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _keycloakService.SetUserAttributeAsync(keycloakUserId, "tenant_id", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _competitionRepository.AddAsync(Arg.Any<Domain.Entities.Competition>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _keycloakService.SetUserAttributeAsync(keycloakUserId, "competition_id", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _tenantRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(keycloakUserId);

        await _tenantRepository.Received(1).AddAsync(Arg.Is<Tenant>(t =>
            t.OrganizationName == command.OrganizationName &&
            t.Email == command.Email
        ), Arg.Any<CancellationToken>());

        await _competitionRepository.Received(1).AddAsync(Arg.Is<Domain.Entities.Competition>(c =>
            c.Name == command.CompetitionName
        ), Arg.Any<CancellationToken>());

        await _keycloakService.Received(1).AssignRoleAsync(keycloakUserId, "organizer", Arg.Any<CancellationToken>());
        await _keycloakService.Received(1).SetUserAttributeAsync(keycloakUserId, "tenant_id", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _keycloakService.Received(1).SetUserAttributeAsync(keycloakUserId, "competition_id", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "existing@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        var existingTenantResult = Tenant.Create(
            organizationName: "Existing Org",
            email: "existing@example.com"
        );
        var existingTenant = existingTenantResult.Value!;

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(existingTenant));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already");

        await _keycloakService.DidNotReceiveWithAnyArgs().CreateUserAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Handle_KeycloakUserCreationFails_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(null));

        _keycloakService.CreateUserAsync(command.Email, command.Password, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Failure("Keycloak error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Keycloak error");

        await _tenantRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_RoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(null));

        _keycloakService.CreateUserAsync(command.Email, command.Password, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        _keycloakService.AssignRoleAsync(keycloakUserId, "organizer", Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Role assignment failed"));

        _keycloakService.DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Role assignment failed");

        await _keycloakService.Received(1).DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>());
        await _tenantRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_DatabaseSaveFails_DeletesUserAndReturnsFailure()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(null));

        _keycloakService.CreateUserAsync(command.Email, command.Password, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        _keycloakService.AssignRoleAsync(keycloakUserId, "organizer", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _tenantRepository.AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _keycloakService.SetUserAttributeAsync(keycloakUserId, "tenant_id", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _competitionRepository.AddAsync(Arg.Any<Domain.Entities.Competition>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _keycloakService.SetUserAttributeAsync(keycloakUserId, "competition_id", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _tenantRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        _keycloakService.DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Database error");

        await _keycloakService.Received(1).DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetTenantIdAttributeFails_DeletesUserAndReturnsFailure()
    {
        // Arrange
        var command = new RegisterOrganizerCommand(
            Email: "organizer@example.com",
            Password: "SecurePass123",
            OrganizationName: "Test Organization",
            CompetitionName: "Spring Classic 2026",
            PlanName: "TRIAL"
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        _tenantRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Tenant?>(null));

        _keycloakService.CreateUserAsync(command.Email, command.Password, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(keycloakUserId));

        _keycloakService.AssignRoleAsync(keycloakUserId, "organizer", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _tenantRepository.AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _keycloakService.SetUserAttributeAsync(keycloakUserId, "tenant_id", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Attribute error"));

        _keycloakService.DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Attribute error");

        await _keycloakService.Received(1).DeleteUserAsync(keycloakUserId, Arg.Any<CancellationToken>());
    }
}
