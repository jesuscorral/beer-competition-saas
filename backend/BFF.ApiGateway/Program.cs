using BeerCompetition.BFF.ApiGateway.Extensions;
using BeerCompetition.BFF.ApiGateway.Middleware;
using BeerCompetition.BFF.ApiGateway.Policies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure observability (Serilog, OpenTelemetry)
builder.AddObservability();

// Add CORS
builder.Services.AddBffCors(builder.Configuration);

// Add authentication with Keycloak JWT validation
builder.Services.AddBffAuthentication(builder.Configuration);

// Add authorization policies
builder.Services.AddBffAuthorizationPolicies();

// Add token exchange service
builder.Services.AddTokenExchange(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

// Add YARP reverse proxy with token exchange transforms
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTokenExchangeTransforms(builder.Configuration)
    .ConfigureHttpClient((context, handler) =>
    {
        // Configure YARP's SocketsHttpHandler for optimal performance
        handler.ConfigureForYarp();
    });

var app = builder.Build();

// Custom middleware (must run before observability so correlation ID is available for logging)
app.UseMiddleware<CorrelationIdMiddleware>();

// Observability middleware
app.UseObservability();

// CORS
app.UseBffCors();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Tenant extraction (after authentication)
app.UseMiddleware<TenantExtractionMiddleware>();

// Health check endpoint (allow anonymous access for monitoring systems)
app.MapHealthChecks("/health").AllowAnonymous();

// YARP reverse proxy routes
app.MapReverseProxy();

try
{
    Log.Information("Starting BFF API Gateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BFF API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
