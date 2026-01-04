# Database Schema Visualization

## Entity Relationship Diagram

```
┌────────────────────────────────────────────────────────┐
│                    Schema: Competition                  │
└────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────┐
│              Tenant (Aggregate Root)         │
├──────────────────────────────────────────────┤
│ PK  id                      UUID             │
│     tenant_id               UUID (= id)      │
│     organization_name       VARCHAR(255)     │
│ UK  email                   VARCHAR(255)     │
│     status                  VARCHAR(50)      │
│     created_at              TIMESTAMP        │
│     updated_at              TIMESTAMP?       │
└───────────────┬──────────────────────────────┘
                │
                │ 1:N
                │ (One Tenant has Many Competitions)
                │
                ▼
┌──────────────────────────────────────────────┐
│          Competition (Aggregate Root)        │
├──────────────────────────────────────────────┤
│ PK  id                      UUID             │
│ FK  tenant_id               UUID             │
│     name                    VARCHAR(255)     │
│     description             VARCHAR(2000)?   │
│     registration_deadline   TIMESTAMP        │
│     judging_start_date      TIMESTAMP        │
│     judging_end_date        TIMESTAMP?       │
│     status                  VARCHAR(50)      │
│     max_entries_per_entrant INT (default 10) │
│     created_at              TIMESTAMP        │
│     updated_at              TIMESTAMP?       │
└──────────────────────────────────────────────┘

Foreign Key Constraint:
  competitions.tenant_id → tenants.id (ON DELETE RESTRICT)

Indexes:
  • tenants.ix_tenants_email (UNIQUE)
  • tenants.ix_tenants_status
  • competitions.ix_competitions_tenant_id
  • competitions.ix_competitions_tenant_status (tenant_id, status)
  • competitions.ix_competitions_registration_deadline

Check Constraints:
  • tenants: CHECK (tenant_id = id)
  • competitions: CHECK (registration_deadline < judging_start_date)
```

## Multi-Tenancy Flow

```
┌─────────────┐
│   HTTP      │
│  Request    │
└──────┬──────┘
       │
       ▼
┌──────────────────────────────────────┐
│       TenantProvider                 │
│  (Resolves Tenant ID)                │
├──────────────────────────────────────┤
│  Priority:                           │
│  1. X-Tenant-ID header               │
│  2. JWT claim "tenant_id"            │
│  3. HttpContext.Items["TenantId"]    │
│  4. Default Dev Tenant (Dev only)    │
└──────┬───────────────────────────────┘
       │
       │ CurrentTenantId = "11111111-..."
       ▼
┌──────────────────────────────────────┐
│    CompetitionDbContext              │
│  (Global Query Filters)              │
├──────────────────────────────────────┤
│  • Competitions:                     │
│    WHERE tenant_id = @currentTenantId│
│                                      │
│  • Tenants:                          │
│    WHERE tenant_id = @currentTenantId│
└──────┬───────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────┐
│       PostgreSQL Database            │
│                                      │
│  Competition.tenants                 │
│    ├─ 11111111-... (Dev Org)        │
│    └─ 22222222-... (Prod Org 1)     │
│                                      │
│  Competition.competitions            │
│    ├─ Spring Classic 2025            │
│    │  └─ tenant_id: 11111111-...    │
│    └─ Fall Championship 2025         │
│       └─ tenant_id: 22222222-...    │
└──────────────────────────────────────┘
```

## Data Flow: Create Competition

```
1. User Request
   ↓
   POST /api/competitions
   Body: { name: "Spring Classic 2025", ... }
   Headers: [No X-Tenant-ID] (Development mode)

2. TenantProvider Resolution
   ↓
   • Check X-Tenant-ID header → Not found
   • Check JWT claim → Not found
   • Check HttpContext.Items → Not found
   • Check Environment → Development
   • ✅ Use default: "11111111-1111-1111-1111-111111111111"

3. CreateCompetitionHandler
   ↓
   tenantId = _tenantProvider.CurrentTenantId
   // "11111111-1111-1111-1111-111111111111"
   
   competition = Competition.Create(
       tenantId,
       "Spring Classic 2025",
       ...
   )

4. CompetitionDbContext.SaveChangesAsync
   ↓
   • Detect new Competition entity
   • Auto-set: competition.TenantId = "11111111-..."
   • Log: "Auto-set TenantId for new Competition entity"

5. PostgreSQL Insert
   ↓
   INSERT INTO "Competition".competitions 
   (id, tenant_id, name, ...)
   VALUES (
       '<new-guid>',
       '11111111-1111-1111-1111-111111111111',
       'Spring Classic 2025',
       ...
   )

6. Foreign Key Validation
   ↓
   PostgreSQL checks:
   • Does tenant '11111111-...' exist in tenants table?
   • ✅ Yes → Insert successful
   • ❌ No → Error: "violates foreign key constraint"

7. Response
   ↓
   201 Created
   { "id": "<new-guid>" }
```

## Query Filtering Example

```sql
-- C# Code:
var competitions = await _dbContext.Competitions.ToListAsync();

-- Generated SQL (automatically):
SELECT 
    id,
    tenant_id,
    name,
    description,
    ...
FROM "Competition".competitions
WHERE tenant_id = '11111111-1111-1111-1111-111111111111'
--    ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
--    Automatically injected by Global Query Filter!

-- Result:
-- Only returns competitions belonging to current tenant
-- Tenant A cannot see Tenant B's competitions
```

## Tenant Self-Reference Pattern

```
Why tenant_id = id for Tenant entity?

┌─────────────────────────────────────────┐
│  Tenant (id = A, tenant_id = A)         │
│                                         │
│  • Consistency: All entities have       │
│    tenant_id column                     │
│                                         │
│  • Query filters work uniformly:       │
│    WHERE tenant_id = @currentTenantId   │
│                                         │
│  • No special cases needed              │
│                                         │
│  • Tenant A with CurrentTenantId = A:   │
│    - Can see itself (tenant_id = A)    │
│    - Cannot see Tenant B (tenant_id=B) │
└─────────────────────────────────────────┘

Example:

tenants table:
┌──────────┬────────────┬───────────────┐
│    id    │ tenant_id  │ org_name      │
├──────────┼────────────┼───────────────┤
│ 1111...  │ 1111...    │ Dev Org       │
│ 2222...  │ 2222...    │ Prod Org 1    │
│ 3333...  │ 3333...    │ Prod Org 2    │
└──────────┴────────────┴───────────────┘

When CurrentTenantId = "1111...":
  SELECT * FROM tenants
  WHERE tenant_id = '1111...'
  
  Result: Only Dev Org (1 row)
  ✅ Cannot see other tenants
```

## Security Guarantees

```
┌─────────────────────────────────────────────┐
│       Layer 1: Application (EF Core)        │
│   Global Query Filters                      │
│   ✅ Automatic WHERE tenant_id = @current   │
└────────────┬────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────┐
│       Layer 2: Database (PostgreSQL)        │
│   Foreign Key Constraints                   │
│   ✅ Cannot create orphaned records         │
│   ✅ ON DELETE RESTRICT prevents cascades   │
└────────────┬────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────┐
│       Layer 3: Database (PostgreSQL)        │
│   Check Constraints                         │
│   ✅ Tenant: CHECK (tenant_id = id)         │
│   ✅ Competition: CHECK (dates valid)       │
└─────────────────────────────────────────────┘

Result: Defense in depth
  • Application layer prevents cross-tenant queries
  • Database layer enforces data integrity
  • Even if app code bypasses filters (IgnoreQueryFilters),
    PostgreSQL constraints still protect data
```

## Development vs Production

```
┌──────────────────────────────────────────────┐
│            DEVELOPMENT                       │
├──────────────────────────────────────────────┤
│  Environment: Development                    │
│  Default Tenant: Enabled                     │
│  Tenant ID: 11111111-1111-1111-1111-1111... │
│                                              │
│  Request without X-Tenant-ID:                │
│    ✅ Automatically uses default tenant      │
│    ⚠️  Warning logged                        │
│                                              │
│  Use Case:                                   │
│    • Swagger testing                         │
│    • Local development                       │
│    • Unit tests                              │
└──────────────────────────────────────────────┘

┌──────────────────────────────────────────────┐
│            PRODUCTION                        │
├──────────────────────────────────────────────┤
│  Environment: Production                     │
│  Default Tenant: Disabled                    │
│                                              │
│  Request without X-Tenant-ID:                │
│    ❌ Throws InvalidOperationException       │
│    🔒 Security: Must authenticate            │
│                                              │
│  Valid Sources:                              │
│    • X-Tenant-ID header (from BFF)          │
│    • JWT claim "tenant_id"                  │
│    • Middleware-resolved (public endpoints) │
└──────────────────────────────────────────────┘
```

---

## Legend

- **PK**: Primary Key
- **FK**: Foreign Key
- **UK**: Unique Key
- **?**: Nullable field
- **1:N**: One-to-Many relationship
- **→**: References
- **✅**: Validated/Protected
- **⚠️**: Warning condition
- **❌**: Error condition
