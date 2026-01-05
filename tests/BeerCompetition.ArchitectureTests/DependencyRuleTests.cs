using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce clean architecture dependency rules.
/// Based on ADR-009: Modular Monolith with Vertical Slices and DDD.
/// </summary>
public class DependencyRuleTests
{
    [Fact]
    public void DomainProjects_ShouldNotReferenceEntityFrameworkCore()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain projects must not reference Entity Framework Core. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainProjects_ShouldNotReferenceAspNetCore()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain projects must not reference ASP.NET Core. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationProjects_ShouldNotReferenceAspNetCore()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application projects must not reference ASP.NET Core (should be framework-agnostic). " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationProjects_ShouldNotReferenceEntityFrameworkCore()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Application")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application projects must not reference Entity Framework Core directly. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void InfrastructureProjects_ShouldDependOnDomainOrSharedKernel()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Infrastructure")
            .Should().HaveDependencyOn("BeerCompetition.*.Domain")
            .Or().HaveDependencyOn("BeerCompetition.Shared.Kernel")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure must depend on Domain layer or Shared.Kernel. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void InfrastructureProjects_CanReferenceEntityFrameworkCore()
    {
        // Arrange & Act
        var infrastructureTypes = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Infrastructure")
            .GetTypes()
            .ToList();

        // Assert
        infrastructureTypes.Should().NotBeEmpty(
            "Infrastructure layer should exist and can reference EF Core");

        // This is a documentation test - Infrastructure is allowed to use EF Core
        true.Should().BeTrue("Infrastructure layer is allowed to reference EF Core for persistence");
    }

    [Fact]
    public void SharedKernel_ShouldNotDependOnModules()
    {
        // Arrange & Act
        var result1 = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.Shared.Kernel")
            .Should().NotHaveDependencyOn("BeerCompetition.Competition")
            .GetResult();

        var result2 = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.Shared.Kernel")
            .Should().NotHaveDependencyOn("BeerCompetition.Judging")
            .GetResult();

        // Assert
        result1.IsSuccessful.Should().BeTrue(
            $"Shared.Kernel must not depend on Competition module. " +
            $"Violations: {string.Join(", ", result1.FailingTypeNames ?? [])}");
            
        result2.IsSuccessful.Should().BeTrue(
            $"Shared.Kernel must not depend on Judging module. " +
            $"Violations: {string.Join(", ", result2.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void SharedKernel_ShouldNotDependOnApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.Shared.Kernel")
            .Should().NotHaveDependencyOn("BeerCompetition.*.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Shared.Kernel must not depend on Application layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
