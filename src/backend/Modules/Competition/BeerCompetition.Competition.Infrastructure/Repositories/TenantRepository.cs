using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of ITenantRepository.
/// Handles persistence operations for Tenant aggregate root.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly CompetitionDbContext _dbContext;
    private readonly ILogger<TenantRepository> _logger;

    public TenantRepository(
        CompetitionDbContext dbContext,
        ILogger<TenantRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);

        return await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by email: {Email}", email);

        // IgnoreQueryFilters: Email lookup should be global (not tenant-scoped)
        // Used by RegisterOrganizer to check if email is already taken
        return await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Email == email, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding tenant: {TenantId}", tenant.Id);

        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating tenant: {TenantId}", tenant.Id);

        _dbContext.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving tenant changes");

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
