using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BeerCompetition.Shared.Infrastructure.MultiTenancy;

/// <summary>
/// Interface for accessing the current tenant context in a multi-tenant application.
/// Implementations extract tenant ID from the current HTTP request context.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant ID from the request context.
    /// Throws InvalidOperationException if tenant context is not available.
    /// </summary>
    Guid CurrentTenantId { get; }

    /// <summary>
    /// Attempts to get the current tenant ID.
    /// Returns true if tenant ID is available, false otherwise.
    /// </summary>
    bool TryGetCurrentTenantId(out Guid tenantId);
}

/// <summary>
/// Default implementation of ITenantProvider that extracts tenant ID from HTTP context.
/// Tenant ID resolution priority:
/// 1. X-Tenant-ID header (injected by BFF/API Gateway after JWT validation)
/// 2. JWT claim "tenant_id" (direct authentication scenario)
/// 3. HttpContext.Items["TenantId"] (set by middleware for public endpoints)
/// 4. Default development tenant (ONLY in Development environment)
/// </summary>
public class TenantProvider : ITenantProvider
{
    private const string TenantIdHeaderName = "X-Tenant-ID";
    private const string TenantIdClaimType = "tenant_id";
    private const string DefaultDevelopmentTenantId = "11111111-1111-1111-1111-111111111111";
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantProvider> _logger;

    public TenantProvider(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<TenantProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public Guid CurrentTenantId
    {
        get
        {
            if (TryGetCurrentTenantId(out var tenantId))
                return tenantId;

            _logger.LogError("Tenant context not available. Ensure X-Tenant-ID header or JWT tenant_id claim is present.");
            throw new InvalidOperationException(
                "Tenant context not available. Authentication required or missing X-Tenant-ID header.");
        }
    }

    public bool TryGetCurrentTenantId(out Guid tenantId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext not available");
            tenantId = Guid.Empty;
            return false;
        }

        // Priority 1: X-Tenant-ID header (set by BFF/API Gateway)
        if (httpContext.Request.Headers.TryGetValue(TenantIdHeaderName, out var headerValue))
        {
            if (Guid.TryParse(headerValue.ToString(), out tenantId))
            {
                _logger.LogDebug("Tenant ID {TenantId} extracted from X-Tenant-ID header", tenantId);
                return true;
            }
            
            _logger.LogWarning("Invalid tenant ID in X-Tenant-ID header: {HeaderValue}", headerValue);
        }

        // Priority 2: JWT claim (when direct authentication is enabled)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = httpContext.User.FindFirst(TenantIdClaimType);
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out tenantId))
            {
                _logger.LogDebug("Tenant ID {TenantId} extracted from JWT claim '{ClaimType}'", tenantId, TenantIdClaimType);
                return true;
            }
        }

        // Priority 3: HttpContext.Items (set by middleware for public endpoints)
        if (httpContext.Items.TryGetValue("TenantId", out var contextTenantId) && 
            contextTenantId is Guid tenantIdFromContext)
        {
            tenantId = tenantIdFromContext;
            _logger.LogDebug("Tenant ID {TenantId} extracted from HttpContext.Items", tenantId);
            return true;
        }

        // Priority 4: Development default tenant (ONLY in Development environment)
        if (IsDevelopmentEnvironment())
        {
            tenantId = Guid.Parse(DefaultDevelopmentTenantId);
            _logger.LogWarning(
                "[DEVELOPMENT] No tenant ID found in request. Using default development tenant: {TenantId}", 
                tenantId);
            return true;
        }

        _logger.LogWarning("No tenant ID found in request context (header, JWT claim, or HttpContext.Items)");
        tenantId = Guid.Empty;
        return false;
    }

    /// <summary>
    /// Determines if the application is running in Development environment.
    /// Used to enable development-only features like default tenant.
    /// </summary>
    private bool IsDevelopmentEnvironment()
    {
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
}
