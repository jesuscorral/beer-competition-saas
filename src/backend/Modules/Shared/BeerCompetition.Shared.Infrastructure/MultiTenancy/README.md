# Multi-Tenancy Implementation

## Overview

This module provides multi-tenant support for the Beer Competition SaaS platform. The `TenantProvider` automatically extracts the tenant ID from incoming HTTP requests using multiple fallback strategies.

## Tenant Resolution Priority

The `TenantProvider` resolves the tenant ID in the following order:

1. **X-Tenant-ID Header** (Production)
   - Injected by BFF/API Gateway after JWT validation
   - Example: `X-Tenant-ID: 550e8400-e29b-41d4-a716-446655440000`

2. **JWT Claim** (Direct Authentication)
   - Extracted from `tenant_id` claim in authenticated user's JWT
   - Used when API is accessed directly without BFF

3. **HttpContext.Items** (Public Endpoints)
   - Set by middleware after resolving tenant from competition ID
   - Used for public endpoints like entry registration

4. **Default Development Tenant** (Development Only)
   - Automatically uses `11111111-1111-1111-1111-111111111111`
   - **Only active in Development environment**
   - Enables testing via Swagger without authentication

## Usage

### In Application Handlers

```csharp
public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    private readonly ITenantProvider _tenantProvider;
    
    public async Task<Result<Guid>> Handle(CreateCompetitionCommand cmd, CancellationToken ct)
    {
        // Get current tenant ID automatically
        var tenantId = _tenantProvider.CurrentTenantId;
        
        // Use tenant ID in domain logic
        var competition = Competition.Create(tenantId, cmd.Name, ...);
        
        // ...
    }
}
```

### Testing in Swagger (Development)

In **Development** environment:
1. Open Swagger UI: `https://localhost:5001`
2. Call any endpoint without providing `X-Tenant-ID` header
3. The default tenant `11111111-1111-1111-1111-111111111111` will be used automatically

In **Production** environment:
- Requests **MUST** include `X-Tenant-ID` header or valid JWT with `tenant_id` claim
- Requests without tenant context will fail with `InvalidOperationException`

### Testing with Custom Tenant ID

You can override the default tenant by providing the `X-Tenant-ID` header:

```bash
curl -X POST "https://localhost:5001/api/competitions" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-ID: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "name": "Spring Classic 2025",
    "registrationDeadline": "2025-04-30T23:59:59Z",
    "judgingStartDate": "2025-05-15T09:00:00Z"
  }'
```

## Configuration

### Development Tenant ID

The default development tenant ID is hardcoded in `TenantProvider.cs`:

```csharp
private const string DefaultDevelopmentTenantId = "11111111-1111-1111-1111-111111111111";
```

### Database Setup

Ensure the default tenant exists in your database:

```sql
-- Run the development setup script
psql -h localhost -U postgres -d beercompetition -f docs/database/development-tenant-setup.sql
```

Or manually:

```sql
CREATE SCHEMA IF NOT EXISTS "Competition";

INSERT INTO "Competition".tenants (id, tenant_id, organization_name, email, status, created_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',
    'Development Organization',
    'dev@beercompetition.local',
    'Active',
    NOW()
)
ON CONFLICT (id) DO NOTHING;
```

## Data Model

### Tenant Entity

The `Tenant` entity represents an organization/organizer account:

```csharp
public class Tenant : Entity, IAggregateRoot
{
    public string OrganizationName { get; private set; }
    public string Email { get; private set; }
    public TenantStatus Status { get; private set; }
    
    // Navigation property
    public ICollection<Competition> Competitions { get; private set; }
}
```

**Key characteristics:**
- Each tenant has a unique email
- `tenant_id` = `id` (self-reference for query filter consistency)
- One tenant can own multiple competitions
- Status: Active, Suspended, or Deleted

### Competition Entity

The `Competition` entity belongs to a tenant:

```csharp
public class Competition : Entity, IAggregateRoot
{
    public Guid TenantId { get; private set; }  // Foreign key
    public string Name { get; private set; }
    // ... other properties
    
    // Navigation property
    public Tenant? Tenant { get; private set; }
}
```

**Relationship:**
```
Tenant (1) ──────< (N) Competition
   |
   └─ TenantId (FK)
```

### Database Schema

All tables are in the `Competition` schema:

```
Competition.tenants
  ├─ id (PK)
  ├─ tenant_id (self-reference = id)
  ├─ organization_name
  ├─ email (UNIQUE)
  └─ status

Competition.competitions
  ├─ id (PK)
  ├─ tenant_id (FK -> tenants.id)
  ├─ name
  ├─ status
  └─ ... other fields
```

## Security Considerations

⚠️ **IMPORTANT**: The default development tenant feature is **ONLY** active when:
- `ASPNETCORE_ENVIRONMENT` is set to `Development`
- No `X-Tenant-ID` header is provided
- No JWT with `tenant_id` claim is present

In **Production**:
- Default tenant is **DISABLED**
- All requests **MUST** provide valid tenant context
- Missing tenant context results in `InvalidOperationException`

## Logging

The `TenantProvider` logs tenant resolution at various levels:

- **Debug**: Successful tenant resolution with source
- **Warning**: No tenant found (uses default in Development)
- **Error**: No tenant context available (Production)

Example logs:

```
[DBG] Tenant ID 11111111-1111-1111-1111-111111111111 extracted from X-Tenant-ID header
[WRN] [DEVELOPMENT] No tenant ID found in request. Using default development tenant: 11111111-1111-1111-1111-111111111111
[ERR] Tenant context not available. Ensure X-Tenant-ID header or JWT tenant_id claim is present.
```

## Registration

The `TenantProvider` is registered in the DI container via:

```csharp
services.AddSharedInfrastructure();
```

This also registers `IHttpContextAccessor` which is required for accessing HTTP request context.
