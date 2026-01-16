namespace BeerCompetition.BFF.ApiGateway.Extensions;

/// <summary>
/// Extension methods for configuring CORS to allow frontend access.
/// </summary>
public static class CorsExtensions
{
    private const string BffCorsPolicy = "BffCorsPolicy";

    public static IServiceCollection AddBffCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(BffCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Correlation-ID", "X-Tenant-ID");
            });
        });

        return services;
    }

    public static IApplicationBuilder UseBffCors(this IApplicationBuilder app)
    {
        app.UseCors(BffCorsPolicy);
        return app;
    }
}
