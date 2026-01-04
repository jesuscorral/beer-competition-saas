using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BeerCompetition.Host.Extensions;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication with Keycloak.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakSettings = configuration.GetSection("Keycloak");
        var authority = keycloakSettings["Authority"];
        var audience = keycloakSettings["Audience"];
        var requireHttpsMetadata = keycloakSettings.GetValue<bool>("RequireHttpsMetadata", false);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
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
                RoleClaimType = "roles",
                NameClaimType = "preferred_username"
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(context.Exception, 
                        "JWT Authentication failed for request {Path}", 
                        context.Request.Path);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    var tenantId = context.Principal?.FindFirst("tenant_id")?.Value;
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogInformation(
                        "JWT Token validated for User {UserId}, Tenant {TenantId}", 
                        userId, 
                        tenantId);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

}
