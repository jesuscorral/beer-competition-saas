using BeerCompetition.Shared.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Competition module.
/// Implements multi-tenancy using Global Query Filters.
/// </summary>
public class CompetitionDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CompetitionDbContext> _logger;

    public CompetitionDbContext(
        DbContextOptions<CompetitionDbContext> options,
        ITenantProvider tenantProvider,
        ILogger<CompetitionDbContext> logger)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public DbSet<Domain.Entities.Competition> Competitions => Set<Domain.Entities.Competition>();
    public DbSet<Domain.Entities.Tenant> Tenants => Set<Domain.Entities.Tenant>();
    public DbSet<Domain.Entities.SubscriptionPlan> SubscriptionPlans => Set<Domain.Entities.SubscriptionPlan>();
    public DbSet<Domain.Entities.CompetitionUser> CompetitionUsers => Set<Domain.Entities.CompetitionUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for Competition module
        modelBuilder.HasDefaultSchema("Competition");

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompetitionDbContext).Assembly);

        // Global query filter for multi-tenancy on Competition
        // Automatically injects "WHERE tenant_id = @current_tenant_id" to all queries
        modelBuilder.Entity<Domain.Entities.Competition>()
            .HasQueryFilter(c => c.TenantId == _tenantProvider.CurrentTenantId);

        // Global query filter for multi-tenancy on Tenant
        // Tenant can only see itself (tenant_id = id for Tenant entity)
        modelBuilder.Entity<Domain.Entities.Tenant>()
            .HasQueryFilter(t => t.TenantId == _tenantProvider.CurrentTenantId);

        // Global query filter for multi-tenancy on SubscriptionPlan
        modelBuilder.Entity<Domain.Entities.SubscriptionPlan>()
            .HasQueryFilter(p => p.TenantId == _tenantProvider.CurrentTenantId);

        // Global query filter for multi-tenancy on CompetitionUser
        modelBuilder.Entity<Domain.Entities.CompetitionUser>()
            .HasQueryFilter(cu => cu.TenantId == _tenantProvider.CurrentTenantId);

        _logger.LogDebug("CompetitionDbContext model created with multi-tenancy filter and Competition schema");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set TenantId for new entities automatically
        if (_tenantProvider.TryGetCurrentTenantId(out var tenantId))
        {
            // Handle Competition entities
            var newCompetitions = ChangeTracker.Entries<Domain.Entities.Competition>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in newCompetitions)
            {
                entity.TenantId = tenantId;
                _logger.LogDebug("Auto-set TenantId={TenantId} for new Competition entity", tenantId);
            }

            // Handle Tenant entities (TenantId = Id for self-reference)
            var newTenants = ChangeTracker.Entries<Domain.Entities.Tenant>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in newTenants)
            {
                entity.TenantId = entity.Id;  // Self-reference: tenant_id = id
                _logger.LogDebug("Auto-set TenantId={TenantId} for new Tenant entity (self-reference)", entity.Id);
            }

            // Handle SubscriptionPlan entities
            var newSubscriptionPlans = ChangeTracker.Entries<Domain.Entities.SubscriptionPlan>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in newSubscriptionPlans)
            {
                entity.TenantId = tenantId;
                _logger.LogDebug("Auto-set TenantId={TenantId} for new SubscriptionPlan entity", tenantId);
            }

            // Handle CompetitionUser entities
            var newCompetitionUsers = ChangeTracker.Entries<Domain.Entities.CompetitionUser>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in newCompetitionUsers)
            {
                entity.TenantId = tenantId;
                _logger.LogDebug("Auto-set TenantId={TenantId} for new CompetitionUser entity", tenantId);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
