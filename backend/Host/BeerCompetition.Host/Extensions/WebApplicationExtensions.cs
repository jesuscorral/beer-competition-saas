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
            });
        }

        return app;
    }

    /// <summary>
    /// Configures security middleware (HTTPS, Authentication, Authorization)
    /// </summary>
    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();

        // TODO: Add authentication middleware when Keycloak integration is ready
        // app.UseAuthentication();
        // app.UseAuthorization();

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
        // Future: app.MapJudgingEndpoints();

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        }))
        .WithName("HealthCheck")
        .WithTags("Health");

        return app;
    }
}
