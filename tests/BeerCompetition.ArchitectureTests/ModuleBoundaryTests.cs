using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce module boundaries and prevent unwanted dependencies between modules.
/// Based on ADR-009: Modular Monolith with Vertical Slices and DDD.
/// </summary>
public class ModuleBoundaryTests
{
    private const string CompetitionNamespace = "BeerCompetition.Competition";
    private const string JudgingNamespace = "BeerCompetition.Judging";
    private const string SharedNamespace = "BeerCompetition.Shared";

    [Fact]
    public void DomainLayer_ShouldNotDependOnInfrastructure()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on Infrastructure (EF Core, etc.). " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOnApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain")
            .ShouldNot().HaveDependencyOn("BeerCompetition.*.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on Application layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOnInfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .ShouldNot().HaveDependencyOn("BeerCompetition.*.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not depend on Infrastructure layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainLayer_ShouldOnlyDependOnSharedKernel()
    {
        // Arrange: Allowed dependencies for Domain layer
        var allowedDependencies = new[]
        {
            "BeerCompetition.Shared.Kernel",
            "System",
            "Microsoft.Extensions", // For DI abstractions only
            "MediatR" // For INotification (Domain Events)
        };

        // Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain")
            .And().DoNotHaveName("AssemblyInfo")
            .Should().OnlyHaveDependenciesOn(allowedDependencies)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must only depend on Shared.Kernel and basic system libraries. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void InfrastructureLayer_ShouldDependOnDomainLayer()
    {
        // Arrange & Act
        var infrastructureAssembly = typeof(Competition.Infrastructure.Persistence.CompetitionDbContext).Assembly;

        var infrastructureTypes = Types.InAssembly(infrastructureAssembly)
            .GetTypes()
            .ToList();

        // Assert
        infrastructureTypes.Should().NotBeEmpty(
            "Infrastructure layer should exist and contain types");

        // Verify infrastructure has access to domain (this is allowed and expected)
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Infrastructure")
            .Should().HaveDependencyOn("BeerCompetition.*.Domain")
            .Or().HaveDependencyOn("BeerCompetition.Shared.Kernel")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure layer must depend on Domain layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
