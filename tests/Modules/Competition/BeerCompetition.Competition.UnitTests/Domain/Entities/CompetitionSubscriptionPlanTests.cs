using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Events;

namespace BeerCompetition.Competition.UnitTests.Domain.Entities;

public class CompetitionSubscriptionPlanTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void SetSubscriptionPlan_ValidPlan_UpdatesCompetitionProperties()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            tenantId: _tenantId,
            name: "Spring Classic 2025",
            description: "Annual competition",
            registrationDeadline: DateTime.UtcNow.AddDays(30),
            judgingStartDate: DateTime.UtcNow.AddDays(31),
            judgingEndDate: DateTime.UtcNow.AddDays(32)
        ).Value;

        var plan = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value;

        // Act
        var result = competition.SetSubscriptionPlan(plan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        competition.SubscriptionPlanId.Should().Be(plan.Id);
        competition.MaxEntries.Should().Be(50);
        competition.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void SetSubscriptionPlan_CompetitionAlreadyHasPlan_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId, "Competition", null, DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), null).Value;

        var plan1 = SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR").Value;
        var plan2 = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value;

        competition.SetSubscriptionPlan(plan1);

        // Act
        var result = competition.SetSubscriptionPlan(plan2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already has a subscription plan");
    }

    [Fact]
    public void SetSubscriptionPlan_NullPlan_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId, "Competition", null, DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), null).Value;

        // Act
        var result = competition.SetSubscriptionPlan(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void SetSubscriptionPlan_PlanFromDifferentTenant_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId, "Competition", null, DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), null).Value;

        var differentTenantId = Guid.NewGuid();
        var plan = SubscriptionPlan.Create(differentTenantId, "BASIC", 50, 49.00m, "EUR").Value;

        // Act
        var result = competition.SetSubscriptionPlan(plan);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("same tenant");
    }

    [Fact]
    public void SetSubscriptionPlan_RaisesDomainEvent()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId, "Competition", null, DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), null).Value;

        var plan = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value;

        // Clear the CompetitionCreatedEvent to test only the SetSubscriptionPlan event
        competition.ClearDomainEvents();

        // Act
        competition.SetSubscriptionPlan(plan);

        // Assert
        var domainEvent = competition.DomainEvents
            .Should().ContainSingle()
            .Which.Should().BeOfType<SubscriptionPlanSelectedEvent>().Subject;

        domainEvent.CompetitionId.Should().Be(competition.Id);
        domainEvent.SubscriptionPlanId.Should().Be(plan.Id);
        domainEvent.MaxEntries.Should().Be(50);
    }

    [Theory]
    [InlineData("TRIAL", 10)]
    [InlineData("BASIC", 50)]
    [InlineData("STANDARD", 200)]
    [InlineData("PRO", 600)]
    public void SetSubscriptionPlan_AllPlanTypes_WorkCorrectly(string planName, int expectedMaxEntries)
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId, "Competition", null, DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), null).Value;

        var plan = SubscriptionPlan.Create(_tenantId, planName, expectedMaxEntries, 0.00m, "EUR").Value;

        // Act
        var result = competition.SetSubscriptionPlan(plan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        competition.MaxEntries.Should().Be(expectedMaxEntries);
        competition.IsPublic.Should().BeTrue();
    }
}
