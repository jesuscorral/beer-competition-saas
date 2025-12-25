using BeerCompetition.BFF.ApiGateway.Services;
using BeerCompetition.BFF.ApiGateway.Transforms;
using Yarp.ReverseProxy.Transforms;

namespace BeerCompetition.BFF.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring token exchange in YARP reverse proxy.
/// Adds transforms to automatically exchange frontend tokens for service-specific tokens.
/// </summary>
public static class TokenExchangeExtensions
{
    /// <summary>
    /// Adds token exchange service and configures YARP transforms for all routes.
    /// </summary>
    public static IServiceCollection AddTokenExchange(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register token exchange service
        services.AddHttpClient();
        services.AddSingleton<ITokenExchangeService, KeycloakTokenExchangeService>();

        return services;
    }

    /// <summary>
    /// Configures YARP to use token exchange transforms based on route configuration.
    /// </summary>
    public static IReverseProxyBuilder AddTokenExchangeTransforms(
        this IReverseProxyBuilder builder,
        IConfiguration configuration)
    {
        var serviceClients = configuration.GetSection("ServiceClients");

        builder.AddTransforms(builderContext =>
        {
            // Determine target audience based on cluster
            var clusterId = builderContext.Route.ClusterId;
            string? targetAudience = null;

            if (clusterId?.Contains("competition", StringComparison.OrdinalIgnoreCase) == true)
            {
                targetAudience = serviceClients["CompetitionService:Audience"];
            }
            else if (clusterId?.Contains("judging", StringComparison.OrdinalIgnoreCase) == true)
            {
                targetAudience = serviceClients["JudgingService:Audience"];
            }

            // Add token exchange transform if target audience is configured
            if (!string.IsNullOrWhiteSpace(targetAudience))
            {
                builderContext.AddRequestTransform(async transformContext =>
                {
                    var tokenExchangeService = transformContext.HttpContext.RequestServices
                        .GetRequiredService<ITokenExchangeService>();
                    var logger = transformContext.HttpContext.RequestServices
                        .GetRequiredService<ILogger<TokenExchangeTransform>>();

                    var transform = new TokenExchangeTransform(
                        tokenExchangeService,
                        logger,
                        targetAudience);

                    await transform.ApplyAsync(transformContext);
                });
            }
        });

        return builder;
    }
}
