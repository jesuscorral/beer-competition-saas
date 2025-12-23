using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BeerCompetition.BFF.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring authentication with Keycloak.
/// Implements JWT Bearer authentication as defined in ADR-004.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var keycloakSettings = configuration.GetSection("Keycloak");
        var authority = keycloakSettings["Authority"];
        var audience = keycloakSettings["Audience"];
        var requireHttpsMetadata = keycloakSettings.GetValue<bool>("RequireHttpsMetadata", false);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = keycloakSettings.GetValue<bool>("ValidateIssuer", true),
                    ValidateAudience = keycloakSettings.GetValue<bool>("ValidateAudience", true),
                    ValidateLifetime = keycloakSettings.GetValue<bool>("ValidateLifetime", true),
                    ValidateIssuerSigningKey = keycloakSettings.GetValue<bool>("ValidateIssuerSigningKey", true),
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RoleClaimType = "roles" // Keycloak uses 'roles' claim
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(context.Exception, 
                            "Authentication failed for request {Path}", 
                            context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        var tenantId = context.Principal?.FindFirst("tenant_id")?.Value;
                        var userId = context.Principal?.FindFirst("sub")?.Value;
                        logger.LogDebug(
                            "Token validated for User {UserId}, Tenant {TenantId}", 
                            userId, 
                            tenantId);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
