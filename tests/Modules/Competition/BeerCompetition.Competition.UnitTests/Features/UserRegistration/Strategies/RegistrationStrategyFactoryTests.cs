using BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BeerCompetition.Competition.UnitTests.Features.UserRegistration.Strategies;

public class RegistrationStrategyFactoryTests
{
    private readonly RegistrationStrategyFactory _sut;
    private readonly IRegistrationStrategy[] _strategies;

    public RegistrationStrategyFactoryTests()
    {
        // Create real strategies (they don't have complex dependencies)
        var entrantStrategy = new EntrantRegistrationStrategy();
        var judgeStrategy = new JudgeRegistrationStrategy();
        var stewardStrategy = new StewardRegistrationStrategy();
        var organizerStrategy = new OrganizerRegistrationStrategy(
            Substitute.For<ITenantRepository>(),
            Substitute.For<ILogger<OrganizerRegistrationStrategy>>());

        _strategies = new IRegistrationStrategy[]
        {
            entrantStrategy,
            judgeStrategy,
            stewardStrategy,
            organizerStrategy
        };

        _sut = new RegistrationStrategyFactory(_strategies);
    }

    [Fact]
    public void GetStrategy_WithEntrantRole_ShouldReturnEntrantStrategy()
    {
        // Act
        var result = _sut.GetStrategy(CompetitionUserRole.ENTRANT);

        // Assert
        result.Should().BeOfType<EntrantRegistrationStrategy>();
        result.Role.Should().Be(CompetitionUserRole.ENTRANT);
    }

    [Fact]
    public void GetStrategy_WithJudgeRole_ShouldReturnJudgeStrategy()
    {
        // Act
        var result = _sut.GetStrategy(CompetitionUserRole.JUDGE);

        // Assert
        result.Should().BeOfType<JudgeRegistrationStrategy>();
        result.Role.Should().Be(CompetitionUserRole.JUDGE);
    }

    [Fact]
    public void GetStrategy_WithStewardRole_ShouldReturnStewardStrategy()
    {
        // Act
        var result = _sut.GetStrategy(CompetitionUserRole.STEWARD);

        // Assert
        result.Should().BeOfType<StewardRegistrationStrategy>();
        result.Role.Should().Be(CompetitionUserRole.STEWARD);
    }

    [Fact]
    public void GetStrategy_WithOrganizerRole_ShouldReturnOrganizerStrategy()
    {
        // Act
        var result = _sut.GetStrategy(CompetitionUserRole.ORGANIZER);

        // Assert
        result.Should().BeOfType<OrganizerRegistrationStrategy>();
        result.Role.Should().Be(CompetitionUserRole.ORGANIZER);
    }

    [Fact]
    public void GetStrategy_WithInvalidRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidRole = (CompetitionUserRole)999;

        // Act
        Action act = () => _sut.GetStrategy(invalidRole);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No registration strategy found*");
    }
}
