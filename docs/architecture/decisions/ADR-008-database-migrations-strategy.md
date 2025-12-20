# ADR-008: Code-First Database Migrations Strategy

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

The Beer Competition platform requires a robust database schema management strategy that:
- Supports rapid iterative development
- Enforces multi-tenant data isolation at the database level
- Enables automated CI/CD deployments
- Provides version control for database schema changes
- Allows rollback capabilities for failed deployments
- Maintains consistency across development, staging, and production environments

Should we use **Code-First** (migrations generated from C# entity classes) or **Database-First** (schema defined in SQL scripts) approach?

---

## Decision Drivers

- **Developer Experience**: Developers work in C# code, not SQL
- **Type Safety**: Entity classes provide compile-time safety
- **Multi-Tenancy**: Row-Level Security (RLS) policies must be consistently applied
- **CI/CD Integration**: Migrations must be automated in deployment pipelines
- **Version Control**: All schema changes tracked in git
- **Rollback Capability**: Ability to revert migrations safely
- **Team Collaboration**: Multiple developers working on different features
- **PostgreSQL-Specific Features**: RLS policies, indexes, JSONB optimizations

---

## Considered Options

### 1. **Entity Framework Core Code-First Migrations** (Chosen)
Generate migrations from C# entity classes using `dotnet ef migrations add`.

**Pros:**
- ✅ Entities are the source of truth (single source of truth in code)
- ✅ Type-safe: Compiler catches model changes
- ✅ Automated migration generation with `dotnet ef`
- ✅ Built-in rollback support: `dotnet ef database update <previous-migration>`
- ✅ Excellent for rapid development and prototyping
- ✅ Integrates seamlessly with .NET CI/CD pipelines
- ✅ Migration history tracked in `__EFMigrationsHistory` table

**Cons:**
- ⚠️ PostgreSQL-specific features (RLS, advanced indexes) require raw SQL in migrations
- ⚠️ Complex queries must be hand-tuned
- ⚠️ Requires careful review of generated migrations

---

### 2. **Database-First with SQL Scripts**
Manually write SQL migration scripts in numbered files (e.g., `001_initial_schema.sql`).

**Pros:**
- ✅ Full control over SQL syntax
- ✅ Easy to optimize PostgreSQL-specific features
- ✅ No ORM abstraction layer

**Cons:**
- ❌ Manual synchronization between C# entities and database schema
- ❌ No compile-time validation of entity mappings
- ❌ Higher risk of drift between code and database
- ❌ More manual work for each change
- ❌ Harder to integrate with EF Core

---

### 3. **Hybrid: Code-First + Custom SQL Migrations**
Use EF Core migrations as the baseline, but enhance with raw SQL for PostgreSQL-specific features.

**Pros:**
- ✅ Best of both worlds: EF Core convenience + PostgreSQL power
- ✅ Type-safe entity definitions
- ✅ Custom SQL for RLS, indexes, triggers

**Cons:**
- ⚠️ Requires discipline to maintain both approaches
- ⚠️ Migration files can become complex

---

## Decision Outcome

**Chosen Option: Entity Framework Core Code-First Migrations with PostgreSQL-Specific Enhancements (Hybrid Approach)**

We will use **EF Core Code-First Migrations** as the primary schema management strategy, with **custom SQL enhancements** for PostgreSQL-specific features.

### Implementation Strategy

#### 1. **Entity Definition (Code-First)**

All entities defined in C# with Fluent API configuration:

```csharp
// Domain/Entities/Competition.cs
public class Competition
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }  // REQUIRED for multi-tenancy
    public string Name { get; private set; } = string.Empty;
    public DateTime RegistrationDeadline { get; private set; }
    public CompetitionStatus Status { get; private set; }
    
    // Navigation properties
    public ICollection<Entry> Entries { get; private set; } = new List<Entry>();
    
    // Factory method (DDD pattern)
    public static Competition Create(Guid tenantId, string name, DateTime deadline)
    {
        // Validation logic
        return new Competition { TenantId = tenantId, Name = name, RegistrationDeadline = deadline };
    }
}

// Infrastructure/EntityConfigurations/CompetitionConfiguration.cs
public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("competitions");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        
        // Multi-tenancy: ALWAYS include tenant_id
        builder.Property(c => c.TenantId).IsRequired();
        builder.HasIndex(c => c.TenantId);
        
        builder.Property(c => c.Name).IsRequired().HasMaxLength(255);
        builder.Property(c => c.RegistrationDeadline).IsRequired();
        
        // Global query filter for multi-tenancy
        builder.HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == TenantContext.CurrentTenantId);
    }
}
```

#### 2. **Generate Migrations**

```bash
# From services/competition/ directory
dotnet ef migrations add InitialCreate --context ApplicationDbContext --output-dir Infrastructure/Migrations

# Review generated migration BEFORE applying
# File: Infrastructure/Migrations/<timestamp>_InitialCreate.cs
```

**CRITICAL: Always review generated migrations for:**
- Correct column types
- Proper indexes (especially on `tenant_id`)
- Foreign key constraints
- Missing PostgreSQL-specific features

#### 3. **Enhance Migrations with PostgreSQL-Specific Features**

After EF Core generates the migration, enhance it with raw SQL for RLS policies:

```csharp
// Infrastructure/Migrations/<timestamp>_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // EF Core generated schema
        migrationBuilder.CreateTable(
            name: "competitions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                registration_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_competitions", x => x.id);
            });

        // Index for multi-tenancy queries
        migrationBuilder.CreateIndex(
            name: "ix_competitions_tenant_id",
            table: "competitions",
            column: "tenant_id");

        // ===== PostgreSQL Row-Level Security (RLS) =====
        // CRITICAL: Enforce multi-tenant isolation at database level
        
        migrationBuilder.Sql(@"
            -- Enable Row-Level Security
            ALTER TABLE competitions ENABLE ROW LEVEL SECURITY;

            -- Policy: Users can only access data from their tenant
            CREATE POLICY tenant_isolation ON competitions
                USING (tenant_id = current_setting('app.current_tenant')::uuid);

            -- Policy: Service account can access all data (for background workers)
            CREATE POLICY service_account_access ON competitions
                USING (current_user = 'beercomp_service')
                WITH CHECK (true);
        ");

        // ===== Performance Optimizations =====
        migrationBuilder.Sql(@"
            -- Composite index for tenant + status queries (common pattern)
            CREATE INDEX ix_competitions_tenant_status ON competitions (tenant_id, status);

            -- Update statistics for query planner
            ANALYZE competitions;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop RLS policies first
        migrationBuilder.Sql(@"
            DROP POLICY IF EXISTS tenant_isolation ON competitions;
            DROP POLICY IF EXISTS service_account_access ON competitions;
        ");

        // EF Core generated rollback
        migrationBuilder.DropTable(name: "competitions");
    }
}
```

#### 4. **Apply Migrations**

```bash
# Local development
dotnet ef database update --context ApplicationDbContext

# CI/CD pipeline (Azure Container Apps startup)
dotnet YourService.API.dll --migrate
```

**Startup Migration Logic** (Program.cs):

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... service registrations ...

var app = builder.Build();

// Apply migrations on startup (idempotent)
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // CRITICAL: Set tenant context to null for migrations (bypass RLS)
    var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
    tenantProvider.SetSystemContext();  // Use service account
    
    await dbContext.Database.MigrateAsync();
}

app.Run();
```

#### 5. **Multi-Tenancy Configuration (PostgreSQL Session Context)**

**CRITICAL**: Before every database operation, set the `app.current_tenant` PostgreSQL session variable:

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set PostgreSQL session variable for RLS
        await SetTenantContextAsync(cancellationToken);
        
        // Auto-inject TenantId on new entities
        InjectTenantId();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task SetTenantContextAsync(CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (tenantId != Guid.Empty)
        {
            await Database.ExecuteSqlRawAsync(
                "SET LOCAL app.current_tenant = {0}", 
                tenantId.ToString(), 
                cancellationToken);
        }
    }

    private void InjectTenantId()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty)
            {
                tenantEntity.TenantId = tenantId;
            }
        }
    }
}
```

#### 6. **Migration Workflow (Developer Guide)**

```bash
# 1. Create entity classes and configurations
# 2. Generate migration
dotnet ef migrations add <MigrationName> --context ApplicationDbContext --output-dir Infrastructure/Migrations

# 3. Review generated migration file
# 4. Add PostgreSQL-specific SQL (RLS policies, indexes)
# 5. Test migration locally
dotnet ef database update --context ApplicationDbContext

# 6. Verify schema
psql -h localhost -U dev_user -d beercomp -c "\d+ competitions"

# 7. Test rollback
dotnet ef database update <PreviousMigration> --context ApplicationDbContext

# 8. Commit migration to git
git add Infrastructure/Migrations/<timestamp>_*.cs
git commit -m "feat: add competitions table with RLS policies (#issue-number)"
```

#### 7. **CI/CD Integration (GitHub Actions)**

```yaml
# .github/workflows/deploy.yml
jobs:
  migrate-database:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Install EF Core CLI
        run: dotnet tool install --global dotnet-ef

      - name: Apply migrations
        run: |
          cd services/competition
          dotnet ef database update --context ApplicationDbContext --connection "${{ secrets.PROD_DATABASE_CONNECTION_STRING }}"
        env:
          ASPNETCORE_ENVIRONMENT: Production
```

#### 8. **Migration Testing (Integration Tests)**

```csharp
// Tests/Integration/MigrationTests.cs
public class MigrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MigrationTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task MigrationsApply_Successfully()
    {
        // Arrange: Fresh database with Testcontainers
        await using var dbContext = _fixture.CreateDbContext();

        // Act: Apply all migrations
        await dbContext.Database.MigrateAsync();

        // Assert: Verify schema
        var tableExists = await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM information_schema.tables WHERE table_name = 'competitions'");
        Assert.True(tableExists > 0);
    }

    [Fact]
    public async Task RLSPolicy_EnforcesIsolation()
    {
        // Arrange: Two tenants
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        await using var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync($"SET app.current_tenant = '{tenant1}'");

        var competition1 = Competition.Create(tenant1, "Tenant 1 Competition", DateTime.UtcNow.AddDays(30));
        dbContext.Competitions.Add(competition1);
        await dbContext.SaveChangesAsync();

        // Act: Switch tenant context
        await dbContext.Database.ExecuteSqlRawAsync($"SET app.current_tenant = '{tenant2}'");

        // Assert: Tenant 2 cannot see Tenant 1's data
        var visible = await dbContext.Competitions.AnyAsync(c => c.Id == competition1.Id);
        Assert.False(visible);
    }
}
```

---

## Consequences

### Positive

✅ **Single Source of Truth**: Entity classes are the authoritative schema definition  
✅ **Type Safety**: Compiler validates entity mappings and relationships  
✅ **Automated**: `dotnet ef migrations add` generates migrations automatically  
✅ **Version Controlled**: All schema changes tracked in git  
✅ **Rollback Support**: `dotnet ef database update <previous>` for safe rollbacks  
✅ **CI/CD Ready**: Migrations applied automatically on deployment  
✅ **Multi-Tenancy Enforced**: RLS policies ensure database-level isolation  
✅ **Developer Friendly**: No manual SQL schema synchronization  

### Negative

⚠️ **PostgreSQL-Specific Features**: RLS, advanced indexes require raw SQL in migrations  
⚠️ **Migration Review Required**: Generated migrations must be reviewed before applying  
⚠️ **Learning Curve**: Developers must understand EF Core migration system  
⚠️ **Migration Conflicts**: Multiple developers may create conflicting migrations (resolved via merge strategy)  

### Mitigation Strategies

1. **Code Reviews**: All migrations must be reviewed before merging
2. **Migration Naming Convention**: `<feature>_<description>` (e.g., `CompetitionsTable_AddRLS`)
3. **Test Migrations Locally**: Always test with Testcontainers before pushing
4. **Documentation**: Maintain migration guide in `docs/development/MIGRATIONS.md`
5. **Automated Testing**: Integration tests validate RLS policies and multi-tenancy

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) - Entity Framework Core chosen as ORM
- [ADR-002: Multi-Tenancy Strategy](ADR-002-multi-tenancy-strategy.md) - PostgreSQL RLS enforcement
- [ADR-006: Testing Strategy](ADR-006-testing-strategy.md) - Testcontainers for migration testing

---

## References

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [EF Core with PostgreSQL](https://www.npgsql.org/efcore/)
- [EF Core Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)

---

**Last Updated**: 2025-12-19  
**Next Review**: Post-Sprint 0 (after first production deployment)
