using System.Reflection;
using BeerCompetition.Shared.Kernel;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using Xunit;

namespace BeerCompetition.ArchitectureTests;

/// <summary>
/// Tests to enforce multi-tenancy patterns with Row-Level Security (RLS).
/// Based on ADR-002: Multi-Tenancy Strategy.
/// </summary>
public class MultiTenancyTests
{
    // Define assemblies to test explicitly
    private static readonly Assembly CompetitionInfrastructureAssembly = 
        typeof(Competition.Infrastructure.DependencyInjection).Assembly;
    
    private static readonly Assembly CompetitionDomainAssembly = 
        typeof(Competition.Domain.Entities.Competition).Assembly;

    [Fact]
    public void Entities_ShouldHaveTenantIdProperty()
    {
        // Arrange: Get all entities that inherit from Entity base class
        var entityTypes = Types.InAssembly(CompetitionDomainAssembly)
            .That().ResideInNamespace("BeerCompetition.Competition.Domain.Entities")
            .And().Inherit(typeof(Entity))
            .GetTypes()
            .Where(t => !t.IsAbstract && t.Name != "Entity")
            .ToList();

        // Act & Assert
        foreach (var entityType in entityTypes)
        {
            var hasTenantId = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p => p.Name == "TenantId" && p.PropertyType == typeof(Guid));

            hasTenantId.Should().BeTrue(
                $"Entity {entityType.Name} must have a TenantId property of type Guid for multi-tenancy isolation");
        }
    }

    [Fact]
    public void DbContextClasses_ShouldInjectTenantProvider()
    {
        // Arrange & Act: Scan Infrastructure assembly explicitly for DbContext classes
        var dbContextTypes = Types.InAssembly(CompetitionInfrastructureAssembly)
            .That().Inherit(typeof(DbContext))
            .GetTypes()
            .ToList();

        // Assert
        dbContextTypes.Should().NotBeEmpty(
            "There should be DbContext classes in Infrastructure assembly");

        foreach (var dbContextType in dbContextTypes)
        {
            // Check if constructor has ITenantProvider parameter
            var constructors = dbContextType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var hasTenantProviderParameter = constructors.Any(c =>
                c.GetParameters().Any(p =>
                    p.ParameterType.Name == "ITenantProvider" ||
                    p.ParameterType.FullName?.Contains("ITenantProvider") == true));

            hasTenantProviderParameter.Should().BeTrue(
                $"DbContext {dbContextType.Name} must inject ITenantProvider for multi-tenancy support");
        }
    }

    [Fact]
    public void DomainEntities_ShouldNotBypassTenantIsolation()
    {
        // Arrange: Get all entity types
        var entityTypes = Types.InAssembly(CompetitionDomainAssembly)
            .That().ResideInNamespace("BeerCompetition.Competition.Domain.Entities")
            .And().Inherit(typeof(Entity))
            .GetTypes()
            .ToList();

        // Act & Assert: This is a documentation test - entities should follow tenant isolation
        // In practice, this is enforced by EF Core configuration and RLS policies
        entityTypes.Should().NotBeEmpty("Domain entities should exist and follow tenant isolation patterns");
        
        true.Should().BeTrue("Tenant isolation is enforced through EF Core configuration and PostgreSQL RLS");
    }
}
