using BeerCompetition.Competition.Infrastructure;
using BeerCompetition.Host.Extensions;
using BeerCompetition.Shared.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogConfiguration(builder.Configuration);

// Add Swagger with JWT authentication
builder.Services.AddSwaggerWithJwtAuth(builder.Configuration);

// Add shared infrastructure (HttpContextAccessor + TenantProvider)
builder.Services.AddSharedInfrastructureServices();

// Add modules
builder.Services.AddCompetitionModule(builder.Configuration);
// Future: builder.Services.AddJudgingModule(builder.Configuration);

// Add JWT Authentication with Keycloak
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization Policies
builder.Services.AddAuthorizationPolicies();

var app = builder.Build();

// Apply database migrations (Development only)
app.ApplyDatabaseMigrations();

// Configure the HTTP request pipeline (Swagger)
app.UseApiDocumentation();

// Security middleware
app.UseSecurityMiddleware();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapApplicationEndpoints();

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
