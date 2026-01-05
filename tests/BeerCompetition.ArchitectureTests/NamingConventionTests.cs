using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce naming conventions across the codebase.
/// Based on project conventions and best practices.
/// </summary>
public class NamingConventionTests
{
    [Fact]
    public void Commands_MustEndWithCommand()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application.Features")
            .And().ImplementInterface(typeof(IRequest<>))
            .And().DoNotHaveNameEndingWith("Query")
            .Should().HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Commands must end with 'Command' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Queries_MustEndWithQuery()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application.Features")
            .And().ImplementInterface(typeof(IRequest<>))
            .And().DoNotHaveNameEndingWith("Command")
            .Should().HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Queries must end with 'Query' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Handlers_MustEndWithHandler()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .And().ImplementInterface(typeof(IRequestHandler<,>))
            .Should().HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Handlers must end with 'Handler' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_MustEndWithEvent()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Events")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Event")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain events must end with 'Event' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Repositories_MustEndWithRepository()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Repositories")
            .Should().HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Repository interfaces must end with 'Repository' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Validators_MustEndWithValidator()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .And().HaveNameEndingWith("Validator")
            .Should().HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Validators must end with 'Validator' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Services_MustEndWithService()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Services")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Service")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain services must end with 'Service' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Specifications_MustEndWithSpecification()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Specifications")
            .And().AreClasses()
            .Should().HaveNameEndingWith("Specification")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Specifications must end with 'Specification' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
