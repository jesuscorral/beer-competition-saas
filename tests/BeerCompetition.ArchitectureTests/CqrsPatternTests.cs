using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce CQRS pattern using MediatR.
/// Based on ADR-005: CQRS Implementation.
/// </summary>
public class CqrsPatternTests
{
    [Fact]
    public void Commands_ShouldEndWithCommandSuffix()
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
            $"All commands must end with 'Command' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Queries_ShouldEndWithQuerySuffix()
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
            $"All queries must end with 'Query' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Handlers_ShouldEndWithHandlerSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application.Features")
            .And().ImplementInterface(typeof(IRequestHandler<,>))
            .Should().HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All handlers must end with 'Handler' suffix. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommandsAndQueries_ShouldBeInApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ImplementInterface(typeof(IRequest<>))
            .And().DoNotResideInNamespace("MediatR")
            .Should().ResideInNamespace("BeerCompetition.*.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All commands and queries must be in Application layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Handlers_ShouldBeInApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ImplementInterface(typeof(IRequestHandler<,>))
            .And().DoNotResideInNamespace("MediatR")
            .Should().ResideInNamespace("BeerCompetition.*.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All handlers must be in Application layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommandHandlers_ShouldBeInFeaturesFolder()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .And().HaveNameEndingWith("Handler")
            .And().ImplementInterface(typeof(IRequestHandler<,>))
            .Should().ResideInNamespace("BeerCompetition.*.Application.Features")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All command/query handlers must be in Application/Features folder. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotReferenceAspNetCore()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not reference ASP.NET Core (should be framework-agnostic). " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
