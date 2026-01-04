using BeerCompetition.Shared.Infrastructure.ExternalServices;
using BeerCompetition.Shared.Infrastructure.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace BeerCompetition.Shared.Infrastructure;

/// <summary>
/// Extension methods for registering shared infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers shared infrastructure services including multi-tenancy support.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructureServices(this IServiceCollection services)
    {
        // HTTP context accessor (required for TenantProvider to access request context)
        services.AddHttpContextAccessor();
        
        // Tenant provider with HttpContext integration
        services.AddScoped<ITenantProvider, TenantProvider>();
        
        // Keycloak service with HttpClient
        services.AddHttpClient<IKeycloakService, KeycloakService>();
        
        return services;
    }
}
