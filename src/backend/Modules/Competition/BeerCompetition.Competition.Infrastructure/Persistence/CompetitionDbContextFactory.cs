using BeerCompetition.Shared.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for CompetitionDbContext.
/// Enables EF Core tools (dotnet ef, Add-Migration) to create DbContext instances
/// without requiring a full application startup.
/// </summary>
public class CompetitionDbContextFactory : IDesignTimeDbContextFactory<CompetitionDbContext>
{
    public CompetitionDbContext CreateDbContext(string[] args)
    {
        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<CompetitionDbContext>();
        
        // Use connection string from environment or default for local development
        var connectionString = Environment.GetEnvironmentVariable("MIGRATION_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=beercomp_dev;Username=dev_user;Password=dev_password";
        
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Competition");
        });

        // Create mock tenant provider for design-time
        var tenantProvider = new DesignTimeTenantProvider();
        
        // Create mock logger for design-time
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<CompetitionDbContext>();

        return new CompetitionDbContext(optionsBuilder.Options, tenantProvider, logger);
    }
}

/// <summary>
/// Mock ITenantProvider for design-time operations.
/// Returns a dummy tenant ID since migrations don't need actual tenant context.
/// </summary>
internal class DesignTimeTenantProvider : ITenantProvider
{
    // Use a fixed tenant ID for migrations (doesn't matter which one)
    public Guid CurrentTenantId => Guid.Parse("00000000-0000-0000-0000-000000000001");

    public bool TryGetCurrentTenantId(out Guid tenantId)
    {
        tenantId = CurrentTenantId;
        return true;
    }
}
