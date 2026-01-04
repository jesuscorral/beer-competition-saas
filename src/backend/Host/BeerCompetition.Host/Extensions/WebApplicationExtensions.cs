using BeerCompetition.Competition.API.Endpoints;
using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BeerCompetition.Host.Extensions;

/// <summary>
/// Extension methods for configuring the HTTP request pipeline
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures Swagger/OpenAPI documentation for development
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Beer Competition API v1");
                options.RoutePrefix = string.Empty;  // Serve Swagger at root
                options.DocumentTitle = "Beer Competition API";
                options.DisplayRequestDuration();
                
                // OAuth2 Configuration for Swagger UI with PKCE S256
                options.OAuthUsePkce();  // Enable PKCE with S256 challenge method
                options.OAuthClientId("swagger-ui");  // Dedicated Keycloak client for Swagger OAuth2
                options.OAuthAppName("Beer Competition API");
                options.OAuthScopes("openid", "profile", "email");
            });
        }

        return app;
    }

    /// <summary>
    /// Configures security middleware (HTTPS)
    /// Authentication and Authorization are configured separately in Program.cs
    /// </summary>
    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        return app;
    }

    /// <summary>
    /// Applies database migrations automatically (Development only)
    /// </summary>
    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var competitionDb = scope.ServiceProvider.GetRequiredService<CompetitionDbContext>();
            
            try
            {
                Log.Information("Applying pending database migrations...");
                competitionDb.Database.Migrate();
                Log.Information("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying database migrations");
                throw;
            }
        }

        return app;
    }

    /// <summary>
    /// Maps all module endpoints and health checks
    /// </summary>
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Map module endpoints
        app.MapCompetitionEndpoints();
        app.MapAuthenticationEndpoints();
        // Future: app.MapJudgingEndpoints();

        // Health check endpoint (anonymous access)
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        }))
        .WithName("HealthCheck")
        .WithTags("Health")
        .AllowAnonymous();

        return app;
    }
}
