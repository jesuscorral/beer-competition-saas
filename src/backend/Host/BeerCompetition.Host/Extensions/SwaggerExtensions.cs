using Microsoft.OpenApi.Models;

namespace BeerCompetition.Host.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI with OAuth2 authentication.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwtAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakAuthority = configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/beercomp";

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Beer Competition API - Competition Service",
                Version = "v1",
                Description = "API for managing BJCP-compliant homebrew beer competitions",
                Contact = new OpenApiContact
                {
                    Name = "Beer Competition Platform",
                    Email = "support@beercomp.local"
                }
            });

            // Define OAuth2 security scheme (Authorization Code Flow with PKCE)
            // NOTE: Only standard OIDC scopes are included (openid, profile, email)
            // User roles are automatically included in JWT via Keycloak's 'roles' client scope configuration
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {            
                            { "openid", "OpenID Connect scope" },
                            { "profile", "User profile information" },
                            { "email", "User email address" }
                        }
                    }
                },
                Description = "OAuth2 Authorization Code Flow with PKCE (S256) via Keycloak",
                Extensions = new Dictionary<string, Microsoft.OpenApi.Interfaces.IOpenApiExtension>
                {
                    { "x-pkce", new Microsoft.OpenApi.Any.OpenApiBoolean(true) }
                }
            });

            // Apply security requirement globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email" }
                }
            });
        });

        return services;
    }
}
