# ADR-002: Multi-Tenancy Strategy

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

As a multi-tenant SaaS platform, we must ensure **strict data isolation** between different competition organizers. Each organizer (tenant) must only access their own competitions, entries, judges, and scores. Data leakage between tenants would be catastrophic (e.g., exposing competitor entries or judge assignments).

How do we implement multi-tenancy that guarantees data isolation while maintaining:
- **Security**: No cross-tenant data access, even with application bugs
- **Performance**: Minimal overhead per query
- **Developer Experience**: Simple mental model, hard to misuse
- **Operational Efficiency**: Shared infrastructure (single database cluster)

---

## Decision Drivers

- **Security-in-Depth**: Database-level enforcement, not just application logic
- **Regulatory Compliance**: GDPR data isolation requirements
- **Performance**: No significant query overhead from tenant filtering
- **Cost**: Shared infrastructure cheaper than database-per-tenant
- **Scalability**: Support 1000+ tenants on single PostgreSQL cluster
- **Developer Safety**: Difficult to accidentally write cross-tenant queries

---

## Considered Options

### 1. Separate Database Per Tenant
**Approach**: Each tenant gets own PostgreSQL database

**Pros:**
- ✅ Total isolation (impossible to cross-contaminate)
- ✅ Per-tenant backups/restores
- ✅ Easy to scale individual tenants

**Cons:**
- ❌ **High operational cost**: 1000 databases to manage
- ❌ **Schema migrations** nightmare (deploy to 1000 databases)
- ❌ **Connection pooling** inefficient (1000 connection pools)
- ❌ **Hardware waste**: Most tenants under-utilize resources

---

### 2. Separate Schema Per Tenant
**Approach**: Single database, separate PostgreSQL schema per tenant

**Pros:**
- ✅ Good isolation within shared database
- ✅ Per-tenant schema customization possible

**Cons:**
- ❌ **Schema explosion**: 1000 schemas to manage
- ❌ **Migrations still complex**: Apply to all schemas
- ❌ **Connection pooling**: Must set `search_path` per connection
- ❌ **Query routing**: Application must choose correct schema

---

### 3. Shared Schema with Tenant ID Column
**Approach**: Single database/schema, every table has `tenant_id` column, application filters queries

**Pros:**
- ✅ Simple operational model
- ✅ Easy migrations (single schema)
- ✅ Efficient connection pooling

**Cons:**
- ❌ **Application-level filtering**: Bugs can leak data
- ❌ **Developer burden**: Must remember `WHERE tenant_id = ?` on every query
- ❌ **Accidental cross-tenant queries**: Easy to forget filter

---

### 4. Shared Schema + Tenant ID + PostgreSQL Row-Level Security (RLS)
**Approach**: Option #3 + database-enforced RLS policies

**Pros:**
- ✅ **Security-in-Depth**: Database blocks cross-tenant queries (even if app has bugs)
- ✅ **Simple operations**: Single schema, easy migrations
- ✅ **Developer safety**: ORM can auto-inject filters, RLS provides fallback
- ✅ **Performance**: RLS policies compiled into query planner

**Cons:**
- ❌ **PostgreSQL-specific**: Vendor lock-in (acceptable for this project)
- ❌ **Session configuration**: Must set `app.current_tenant` variable per request

---

## Decision Outcome

**Chosen Option**: **#4 - Shared Schema + Tenant ID + PostgreSQL Row-Level Security (RLS)**

---

## Implementation Details

### 1. Database Schema
Every table (except global reference data) includes `tenant_id`:

```sql
CREATE TABLE competitions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    registration_deadline TIMESTAMP NOT NULL,
    judging_start_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    CONSTRAINT fk_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id)
);

CREATE INDEX idx_competitions_tenant ON competitions(tenant_id);
```

### 2. PostgreSQL Row-Level Security Policies
Enable RLS and create policies that read session variable:

```sql
-- Enable RLS on table
ALTER TABLE competitions ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only see/modify their tenant's data
CREATE POLICY tenant_isolation_policy ON competitions
    USING (tenant_id = current_setting('app.current_tenant')::uuid)
    WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);

-- Grant permissions (RLS applies to all users except superusers)
GRANT SELECT, INSERT, UPDATE, DELETE ON competitions TO app_user;
```

### 3. Entity Framework Core Global Filters
ORM automatically injects `tenant_id` predicate:

```csharp
// Tenant Provider (scoped per HTTP request)
public interface ITenantProvider
{
    Guid CurrentTenantId { get; }
}

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CurrentTenantId
    {
        get
        {
            var tenantClaim = _httpContextAccessor.HttpContext?
                .User.Claims.FirstOrDefault(c => c.Type == "tenant_id");
            
            return tenantClaim != null 
                ? Guid.Parse(tenantClaim.Value) 
                : throw new UnauthorizedAccessException("No tenant context");
        }
    }
}

// DbContext Configuration
public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global filter: Automatically add WHERE tenant_id = @currentTenant
        modelBuilder.Entity<Competition>()
            .HasQueryFilter(c => c.TenantId == _tenantProvider.CurrentTenantId);

        modelBuilder.Entity<Entry>()
            .HasQueryFilter(e => e.TenantId == _tenantProvider.CurrentTenantId);

        // ... repeat for all tenant-scoped entities
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Set PostgreSQL session variable before query execution
        await Database.ExecuteSqlRawAsync(
            $"SET LOCAL app.current_tenant = '{_tenantProvider.CurrentTenantId}'", 
            ct);

        return await base.SaveChangesAsync(ct);
    }
}
```

### 4. BFF (API Gateway) Tenant Injection
Gateway extracts `tenant_id` from JWT and passes to microservices:

```csharp
// Middleware: Extract tenant from JWT
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantClaim = context.User.Claims
            .FirstOrDefault(c => c.Type == "tenant_id");

        if (tenantClaim == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing tenant claim");
            return;
        }

        // Add to request headers for downstream services
        context.Request.Headers["X-Tenant-ID"] = tenantClaim.Value;

        await _next(context);
    }
}
```

### 5. JWT Token Structure
Keycloak issues JWTs with `tenant_id` claim:

```json
{
  "sub": "user-uuid",
  "email": "organizer@example.com",
  "tenant_id": "tenant-uuid-123",
  "roles": ["organizer", "judge"],
  "iat": 1734612000,
  "exp": 1734615600
}
```

---

## Data Flow

```
1. User authenticates → Keycloak issues JWT with tenant_id claim
2. React app includes JWT in Authorization header
3. BFF validates JWT → extracts tenant_id → adds X-Tenant-ID header
4. Microservice receives request → TenantProvider reads tenant_id from claims
5. Entity Framework query filter injects WHERE tenant_id = @current
6. PostgreSQL RLS policy double-checks tenant_id (security-in-depth)
7. Query returns only tenant's data
```

---

## Guarantees

### Security Layer 1: Entity Framework Global Filter
- ✅ Automatically injects `WHERE tenant_id = @current` on all queries
- ✅ Prevents accidental cross-tenant queries in application code
- ⚠️ Can be bypassed with `.IgnoreQueryFilters()` (requires code review)

### Security Layer 2: PostgreSQL RLS
- ✅ **Database-enforced** – even malicious SQL cannot bypass
- ✅ Catches bugs in application layer (defense-in-depth)
- ✅ Applies to all connections (including admin tools)
- ✅ Session variable `app.current_tenant` set per transaction

### Combined Guarantees
- ❌ **Impossible** to access another tenant's data (short of database superuser access)
- ✅ Application bugs cannot cause data leakage
- ✅ Supports compliance audits (GDPR, SOC 2)

---

## Consequences

### Positive
✅ **Security-in-Depth**: Two layers of protection (app + database)  
✅ **Developer Safety**: Hard to write cross-tenant queries  
✅ **Operational Efficiency**: Single database, simple migrations  
✅ **Cost-Effective**: Shared infrastructure, efficient resource usage  
✅ **Performance**: RLS policies compiled into query planner (minimal overhead)  
✅ **Compliance-Ready**: Database-enforced isolation for audits  

### Negative
❌ **PostgreSQL Lock-In**: RLS is PostgreSQL-specific (acceptable trade-off)  
❌ **Session Variable Overhead**: Must set `app.current_tenant` per request  
❌ **Testing Complexity**: Integration tests must mock tenant context  
❌ **Superuser Risk**: Database superusers bypass RLS (requires strict access control)  

### Risks
⚠️ **Missing tenant_id on new tables**: Code review must catch this  
⚠️ **Forgot to set session variable**: Queries fail (fail-safe behavior)  
⚠️ **Global reference data**: Must exclude from RLS (e.g., BJCP styles catalog)  

---

## Alternatives Considered

### Why not database-per-tenant?
- ❌ **Operational nightmare**: 1000+ databases to manage
- ❌ **Migration complexity**: Deploy schema changes to all databases
- ❌ **Cost**: Wasted resources (most tenants under-utilize)

### Why not schema-per-tenant?
- ❌ **Schema explosion**: Still 1000+ schemas to manage
- ❌ **Connection pooling**: Must set `search_path` per connection
- ❌ **Marginal isolation gain**: RLS provides equivalent security

### Why not application-level filtering only?
- ❌ **Bug risk**: Single WHERE clause mistake leaks data
- ❌ **No defense-in-depth**: Application is single point of failure
- ❌ **Compliance issues**: Auditors prefer database-enforced isolation

---

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task Query_FiltersToCurrentTenant()
{
    // Arrange
    _mockTenantProvider.Setup(p => p.CurrentTenantId).Returns(TenantA_Id);
    
    // Act
    var competitions = await _dbContext.Competitions.ToListAsync();
    
    // Assert: Only Tenant A's competitions returned
    competitions.Should().OnlyContain(c => c.TenantId == TenantA_Id);
}
```

### Integration Tests (Testcontainers + PostgreSQL)
```csharp
[Fact]
public async Task RLS_BlocksCrossTenantQuery()
{
    // Arrange: Insert data for Tenant A and Tenant B
    await InsertCompetition(tenantId: TenantA_Id, name: "Tenant A Comp");
    await InsertCompetition(tenantId: TenantB_Id, name: "Tenant B Comp");
    
    // Act: Query as Tenant A
    await _dbContext.Database.ExecuteSqlRawAsync($"SET app.current_tenant = '{TenantA_Id}'");
    var comps = await _dbContext.Competitions.ToListAsync();
    
    // Assert: RLS blocks Tenant B's data
    comps.Should().HaveCount(1);
    comps[0].Name.Should().Be("Tenant A Comp");
}
```

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) (PostgreSQL choice)
- [ADR-004: Authentication & Authorization](ADR-004-authentication-authorization.md) (JWT tenant_id claim)

---

## References

- [PostgreSQL Row-Level Security Documentation](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [Entity Framework Core Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Multi-Tenancy Patterns on Azure](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- [OWASP Multi-Tenancy Security](https://cheatsheetseries.owasp.org/cheatsheets/Multitenant_Cheat_Sheet.html)
