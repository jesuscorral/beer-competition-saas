using BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;
using BeerCompetition.Competition.Domain.Entities;
using FluentAssertions;

namespace BeerCompetition.Competition.UnitTests.Features.UserRegistration.Strategies;

public class JudgeRegistrationStrategyTests
{
    private readonly JudgeRegistrationStrategy _sut;

    public JudgeRegistrationStrategyTests()
    {
        _sut = new JudgeRegistrationStrategy();
    }

    [Fact]
    public void Role_ShouldReturnJudge()
    {
        // Act
        var result = _sut.Role;

        // Assert
        result.Should().Be(CompetitionUserRole.JUDGE);
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

}
