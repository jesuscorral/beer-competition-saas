using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.IntegrationTests.Builders;

/// <summary>
/// Builder for creating Tenant test data with fluent API.
/// Provides sensible defaults with ability to override any property.
/// </summary>
public class TenantBuilder
{
    private string _organizationName = "Test Organization";
    private string _email = $"test-{Guid.NewGuid():N}@example.com";

    public TenantBuilder WithOrganizationName(string organizationName)
    {
        _organizationName = organizationName;
        return this;
    }

    public TenantBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the Tenant entity with all configured values.
    /// Uses domain Create method to ensure business rules are enforced.
    /// </summary>
    public Tenant Build()
    {
        var result = Tenant.Create(_organizationName, _email);
        
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to create Tenant: {result.Error}");
        
        return result.Value!;
    }

    /// <summary>
    /// Creates a new builder instance for fluent chaining.
    /// </summary>
    public static TenantBuilder Default() => new();
}
