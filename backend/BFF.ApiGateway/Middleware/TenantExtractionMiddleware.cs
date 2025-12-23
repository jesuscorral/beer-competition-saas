using System.Security.Claims;

namespace BeerCompetition.BFF.ApiGateway.Middleware;

/// <summary>
/// Middleware to extract tenant_id from JWT claims and inject X-Tenant-ID header to downstream requests.
/// This implements the multi-tenancy strategy from ADR-002.
/// </summary>
public class TenantExtractionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantExtractionMiddleware> _logger;

    public TenantExtractionMiddleware(
        RequestDelegate next,
        ILogger<TenantExtractionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extract tenant_id from JWT claims
            var tenantId = context.User.FindFirstValue("tenant_id");
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? context.User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogWarning("Authenticated user {UserId} missing tenant_id claim", userId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Forbidden", 
                    message = "Missing tenant_id in authentication token" 
                });
                return;
            }

            // Inject X-Tenant-ID header for downstream services
            context.Request.Headers["X-Tenant-ID"] = tenantId;
            context.Request.Headers["X-User-ID"] = userId;

            _logger.LogDebug("Tenant {TenantId} and User {UserId} extracted from JWT", tenantId, userId);
        }

        await _next(context);
    }
}
