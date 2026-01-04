using BeerCompetition.Shared.Infrastructure.MultiTenancy;

namespace BeerCompetition.Competition.IntegrationTests.Infrastructure;

/// <summary>
/// Test implementation of ITenantProvider that allows dynamic tenant switching.
/// Used in integration tests to simulate different tenant contexts.
/// </summary>
public class TestTenantProvider : ITenantProvider
{
    private Guid? _currentTenantId;

    public Guid CurrentTenantId => _currentTenantId 
        ?? throw new InvalidOperationException("Test tenant not set. Call SetTenant() first.");

    /// <summary>
    /// Sets the current tenant context for tests.
    /// </summary>
    public void SetTenant(Guid tenantId)
    {
        _currentTenantId = tenantId;
    }

    /// <summary>
    /// Clears the tenant context (simulates no authentication).
    /// </summary>
    public void ClearTenant()
    {
        _currentTenantId = null;
    }

    public bool TryGetCurrentTenantId(out Guid tenantId)
    {
        if (_currentTenantId.HasValue)
        {
            tenantId = _currentTenantId.Value;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }
}
