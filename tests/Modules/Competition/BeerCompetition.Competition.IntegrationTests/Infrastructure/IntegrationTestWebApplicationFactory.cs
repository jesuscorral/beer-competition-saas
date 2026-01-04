using BeerCompetition.Competition.Infrastructure.Persistence;
using BeerCompetition.Shared.Infrastructure.ExternalServices;
using BeerCompetition.Shared.Infrastructure.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace BeerCompetition.Competition.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Manages Testcontainers lifecycle and configures test services.
/// </summary>
public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("beercomp_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();
    
    // Mock Keycloak service for all tests
    public IKeycloakService KeycloakService { get; private set; } = null!;
    
    // Test tenant provider - allows dynamic tenant switching during tests
    public TestTenantProvider TenantProvider { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the real database registration
            services.RemoveAll<DbContextOptions<CompetitionDbContext>>();
            services.RemoveAll<CompetitionDbContext>();

            // Add test database with Testcontainers connection string
            services.AddDbContext<CompetitionDbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            // Replace ITenantProvider with test implementation
            services.RemoveAll<ITenantProvider>();
            TenantProvider = new TestTenantProvider();
            services.AddSingleton<ITenantProvider>(TenantProvider);

            // Replace Keycloak service with mock
            services.RemoveAll<IKeycloakService>();
            KeycloakService = Substitute.For<IKeycloakService>();
            services.AddSingleton(KeycloakService);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start Postgres container
        await _postgresContainer.StartAsync();

        // Ensure the application is built before accessing Services
        _ = Services; // This triggers WebApplicationFactory to build the application
        
        // Apply migrations
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompetitionDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
