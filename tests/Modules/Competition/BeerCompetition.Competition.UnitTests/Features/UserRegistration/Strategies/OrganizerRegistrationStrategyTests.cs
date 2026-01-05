using BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BeerCompetition.Competition.UnitTests.Features.UserRegistration.Strategies;

public class OrganizerRegistrationStrategyTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<OrganizerRegistrationStrategy> _logger;
    private readonly OrganizerRegistrationStrategy _sut;

    public OrganizerRegistrationStrategyTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _logger = Substitute.For<ILogger<OrganizerRegistrationStrategy>>();
        _sut = new OrganizerRegistrationStrategy(_tenantRepository, _logger);
    }

    [Fact]
    public void Role_ShouldReturnOrganizer()
    {
        // Act
        var result = _sut.Role;

        // Assert
        result.Should().Be(CompetitionUserRole.ORGANIZER);
    }

    [Fact]
    public void GetInitialStatus_ShouldReturnActive()
    {
        // Act
        var result = _sut.GetInitialStatus();

        // Assert
        result.Should().Be(CompetitionUserStatus.ACTIVE);
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_WithOrganizationName_ShouldCreateTenant()
    {
        // Arrange
        var userId = "user-123";
        var tenantId = Guid.NewGuid();
        var organizationName = "My Brew Club";
        var email = "admin@mybrewclub.com";
        var additionalData = new Dictionary<string, string>
        {
            { "organization_name", organizationName },
            { "email", email }
        };

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, null, null, additionalData, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        
        await _tenantRepository.Received(1).AddAsync(
            Arg.Is<Tenant>(t => t.OrganizationName == organizationName),
            Arg.Any<CancellationToken>());
        await _tenantRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_WithoutOrganizationName_ShouldUseDefaultName()
    {
        // Arrange
        var userId = "user-123";
        var additionalData = new Dictionary<string, string>
        {
            { "email", "test@example.com" }
        };

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, null, null, additionalData, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tenantRepository.Received(1).AddAsync(
            Arg.Is<Tenant>(t => t.OrganizationName.Contains("Organization for user")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_WithoutEmail_ShouldUseDefaultEmail()
    {
        // Arrange
        var userId = "user-123";
        var additionalData = new Dictionary<string, string>
        {
            { "organization_name", "Test Org" }
        };

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, null, null, additionalData, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tenantRepository.Received(1).AddAsync(
            Arg.Is<Tenant>(t => t.Email.Contains($"admin@{userId}.local")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_WhenTenantCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = "user-123";
        var additionalData = new Dictionary<string, string>
        {
            { "organization_name", "A" }, // Too short, will fail validation
            { "email", "invalid-email" } // Invalid email
        };

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, null, null, additionalData, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
        
        await _tenantRepository.DidNotReceive().AddAsync(
            Arg.Any<Tenant>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_ShouldLogTenantCreation()
    {
        // Arrange
        var userId = "user-123";
        var organizationName = "Test Organization";
        var additionalData = new Dictionary<string, string>
        {
            { "organization_name", organizationName },
            { "email", "test@example.com" }
        };

        // Act
        await _sut.ExecuteRoleSpecificLogic(userId, null, null, additionalData, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Creating new tenant") && o.ToString()!.Contains(organizationName)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
