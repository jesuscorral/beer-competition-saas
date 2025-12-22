using BeerCompetition.Competition.API.Endpoints;
using BeerCompetition.Competition.Infrastructure;
using BeerCompetition.Competition.Infrastructure.Persistence;
using BeerCompetition.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add shared infrastructure (HttpContextAccessor + TenantProvider)
builder.Services.AddSharedInfrastructure();

// Add modules
builder.Services.AddCompetitionModule(builder.Configuration);
// Future: builder.Services.AddJudgingModule(builder.Configuration);

var app = builder.Build();

// Apply database migrations automatically (Development only - use migration scripts in Production)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
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
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Beer Competition API v1");
        options.RoutePrefix = string.Empty;  // Serve Swagger at root
    });
}

app.UseHttpsRedirection();

// TODO: Add authentication middleware when Keycloak integration is ready
// app.UseAuthentication();
// app.UseAuthorization();

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

try
{
    Log.Information("Starting Beer Competition Host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
