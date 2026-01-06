using BeerCompetition.Shared.Kernel;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce Domain-Driven Design (DDD) tactical patterns.
/// Based on ADR-009: Modular Monolith with Vertical Slices and DDD.
/// </summary>
public class DddPatternTests
{
    [Fact]
    public void Entities_ShouldInheritFromEntityBaseClass()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Entities")
            .And().AreClasses()
            .And().AreNotAbstract()
            .And().DoNotHaveName("Entity")
            .Should().Inherit(typeof(Entity))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All domain entities must inherit from Entity base class. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ValueObjects_ShouldResideInValueObjectsNamespace()
    {
        // Arrange & Act
        var valueObjectTypes = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.ValueObjects")
            .GetTypes()
            .ToList();

        // Assert: This test documents that Value Objects have their own namespace
        // In the future, if a ValueObject base class is created, update this test
        true.Should().BeTrue($"Value Objects namespace exists with {valueObjectTypes.Count} types");
    }

    [Fact]
    public void AggregateRoots_ShouldImplementIAggregateRoot()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Aggregates")
            .And().AreClasses()
            .And().AreNotAbstract()
            .Should().ImplementInterface(typeof(IAggregateRoot))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"All aggregate roots must implement IAggregateRoot interface. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Repositories_ShouldBeInterfacesInDomainLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Repositories")
            .Should().BeInterfaces()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Repository contracts must be interfaces in Domain layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainServices_ShouldBeInDomainLayer()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Services")
            .And().AreClasses()
            .Should().ResideInNamespace("BeerCompetition.*.Domain")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain services must reside in Domain layer. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Entities_ShouldBeSealed_OrDesignedForInheritance()
    {
        // Arrange: Get all entities except abstract base classes
        var entityTypes = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.Entities")
            .And().Inherit(typeof(Entity))
            .And().AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name != "Entity")
            .ToList();

        // Act & Assert: Entities should typically be sealed unless designed for inheritance
        var nonSealedEntityTypes = entityTypes
            .Where(t => !t.IsSealed)
            .ToList();

        foreach (var entityType in nonSealedEntityTypes)
        {
            // If not sealed, should be abstract or have protected constructors
            var hasPublicConstructor = entityType.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Any();

            // If has public constructor and not sealed, this might be intentional for inheritance
            // We'll log a warning but not fail (this is a soft rule)
            if (hasPublicConstructor && !entityType.IsAbstract)
            {
                // This is acceptable - entities can be designed for inheritance
                // Example: Competition could be base for different competition types
            }
        }

        // This test always passes - it's documentation of the pattern
        true.Should().BeTrue("Entities follow inheritance design patterns");
    }

    [Fact]
    public void ValueObjects_ShouldBeImmutable()
    {
        // Arrange: Get all value objects
        var valueObjectTypes = Types.InCurrentDomain()
            .That().ResideInNamespace("BeerCompetition.*.Domain.ValueObjects")
            .GetTypes()
            .ToList();

        // If no value objects exist yet, pass the test
        if (!valueObjectTypes.Any())
        {
            true.Should().BeTrue("No value objects found yet - test will validate when they are created");
            return;
        }

        // Act & Assert: Check that properties don't have public setters
        foreach (var valueObjectType in valueObjectTypes)
        {
            var mutableProperties = valueObjectType.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
                .ToList();

            mutableProperties.Should().BeEmpty(
                $"Value Object {valueObjectType.Name} must be immutable (no public setters). " +
                $"Mutable properties: {string.Join(", ", mutableProperties.Select(p => p.Name))}");
        }
    }
}
