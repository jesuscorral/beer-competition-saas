using BeerCompetition.Competition.Infrastructure;
using BeerCompetition.Host.Extensions;
using BeerCompetition.Shared.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogConfiguration(builder.Configuration);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add shared infrastructure (HttpContextAccessor + TenantProvider)
builder.Services.AddSharedInfrastructureServices();

// Add modules
builder.Services.AddCompetitionModule(builder.Configuration);
// Future: builder.Services.AddJudgingModule(builder.Configuration);

var app = builder.Build();

// Apply database migrations (Development only)
app.ApplyDatabaseMigrations();

// Configure the HTTP request pipeline
app.UseApiDocumentation();
app.UseSecurityMiddleware();
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
