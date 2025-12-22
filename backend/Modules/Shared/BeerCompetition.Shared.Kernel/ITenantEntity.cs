namespace BeerCompetition.Shared.Kernel;

/// <summary>
/// Interface for entities that support multi-tenancy.
/// All domain entities must implement this interface to ensure tenant isolation.
/// </summary>
/// <remarks>
/// Multi-tenancy is enforced at multiple levels:
/// 1. Database: PostgreSQL Row-Level Security (RLS) policies filter by tenant_id
/// 2. ORM: Entity Framework Global Query Filters automatically inject tenant predicate
/// 3. API: BFF/API Gateway extracts tenant_id from JWT and propagates via X-Tenant-ID header
/// </remarks>
public interface ITenantEntity
{
    /// <summary>
    /// Unique identifier of the tenant that owns this entity.
    /// Must be set before persisting to database.
    /// </summary>
    Guid TenantId { get; set; }
}
