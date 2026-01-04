using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace BeerCompetition.Competition.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that provides database cleanup with Respawn.
/// Uses WebApplicationFactory and Testcontainers for real infrastructure.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;

    protected readonly IntegrationTestWebApplicationFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly CompetitionDbContext DbContext;

    protected IntegrationTestBase(IntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<CompetitionDbContext>();
    }

    public async Task InitializeAsync()
    {
        // Apply migrations if not already done (idempotent)
        var pendingMigrations = await DbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await DbContext.Database.MigrateAsync();
        }
        
        // Initialize Respawn once per test
        _dbConnection = new NpgsqlConnection(Factory.ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", "Competition"], // Include both schemas
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task DisposeAsync()
    {
        // Clean database after each test using Respawn (fast!)
        if (_respawner != null && _dbConnection != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
        
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
        
        // Clear tenant context for next test
        Factory.TenantProvider.ClearTenant();
        
        Scope.Dispose();
    }

    /// <summary>
    /// Helper to get a fresh DbContext instance
    /// </summary>
    protected CompetitionDbContext GetFreshDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CompetitionDbContext>();
    }
}
