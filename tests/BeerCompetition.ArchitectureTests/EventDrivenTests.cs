using BeerCompetition.Shared.Kernel;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce event-driven architecture patterns with Outbox Pattern.
/// Based on ADR-003: Event-Driven Architecture.
/// </summary>
public class EventDrivenTests
{
    [Fact]
    public void DomainEvents_ShouldImplementIDomainEvent()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Events")
            .And().HaveNameEndingWith("Event")
            .Should().ImplementInterface(typeof(IDomainEvent))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All domain events must implement IDomainEvent interface. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_MustEndWithEventSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Events")
            .And().ImplementInterface(typeof(IDomainEvent))
            .Should().HaveNameEndingWith("Event")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All domain events must end with 'Event' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Handlers_ShouldNotPublishToRabbitMQDirectly()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application.Features")
            .ShouldNot().HaveDependencyOn("RabbitMQ.Client")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Handlers must use Outbox Pattern, not publish to RabbitMQ directly. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_ShouldBeInDomainLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ImplementInterface(typeof(IDomainEvent))
            .And().DoNotResideInNamespace("BeerCompetition.Shared.Kernel")
            .Should().ResideInNamespace("BeerCompetition.*.Domain")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain events must be in Domain layer (except Shared.Kernel base classes). " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void EventHandlers_ShouldBeInApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().HaveNameEndingWith("EventHandler")
            .And().ResideInNamespace("BeerCompetition.*.Application")
            .Should().ResideInNamespace("BeerCompetition.*.Application.Events")
            .Or().ResideInNamespace("BeerCompetition.*.Application.Features")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Event handlers must be in Application layer (Events or Features folder). " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_ShouldNotDependOnInfrastructure()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ImplementInterface(typeof(IDomainEvent))
            .Should().NotHaveDependencyOn("BeerCompetition.*.Infrastructure")
            .GetResult();

        var result2 = Types.InCurrentDomain()
            .That().ImplementInterface(typeof(IDomainEvent))
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain events must not depend on Infrastructure. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
            
        result2.IsSuccessful.Should().BeTrue(
            $"Domain events must not depend on Entity Framework Core. " +
            $"Violations: {string.Join(", ", result2.FailingTypeNames ?? [])}");
    }
}
