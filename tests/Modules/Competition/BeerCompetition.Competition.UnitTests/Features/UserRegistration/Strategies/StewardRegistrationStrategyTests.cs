using BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;
using BeerCompetition.Competition.Domain.Entities;
using FluentAssertions;

namespace BeerCompetition.Competition.UnitTests.Features.UserRegistration.Strategies;

public class StewardRegistrationStrategyTests
{
    private readonly StewardRegistrationStrategy _sut;

    public StewardRegistrationStrategyTests()
    {
        _sut = new StewardRegistrationStrategy();
    }

    [Fact]
    public void Role_ShouldReturnSteward()
    {
        // Act
        var result = _sut.Role;

        // Assert
        result.Should().Be(CompetitionUserRole.STEWARD);
    }

    [Fact]
    public void GetInitialStatus_ShouldReturnPendingApproval()
    {
        // Act
        var result = _sut.GetInitialStatus();

        // Assert
        result.Should().Be(CompetitionUserStatus.PENDING_APPROVAL);
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_ShouldReturnSuccessWithTenantId()
    {
        // Arrange
        var userId = "user-123";
        var competitionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var additionalData = new Dictionary<string, string>();

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, competitionId, tenantId, additionalData, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tenantId);
    }

    [Fact]
    public async Task ExecuteRoleSpecificLogic_WithNullAdditionalData_ShouldSucceed()
    {
        // Arrange
        var userId = "user-123";
        var competitionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var result = await _sut.ExecuteRoleSpecificLogic(userId, competitionId, tenantId, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tenantId);
    }
}
