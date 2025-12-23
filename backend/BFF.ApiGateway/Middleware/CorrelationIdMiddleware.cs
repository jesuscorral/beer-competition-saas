namespace BeerCompetition.BFF.ApiGateway.Middleware;

/// <summary>
/// Middleware to generate and propagate correlation IDs for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if correlation ID already exists in request
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Propagate to downstream services via request headers
        context.Request.Headers[CorrelationIdHeader] = correlationId;

        // Add to activity for OpenTelemetry
        System.Diagnostics.Activity.Current?.SetTag("correlation_id", correlationId.ToString());

        await _next(context);
    }
}
