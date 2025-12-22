using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeerCompetition.Competition.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Competition aggregate.
/// Delegates persistence logic to Entity Framework Core DbContext.
/// </summary>
internal class CompetitionRepository : ICompetitionRepository
{
    private readonly CompetitionDbContext _context;

    public CompetitionRepository(CompetitionDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically adds tenant_id filter
        return await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.Competition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Global query filter automatically filters by current tenant
        return await _context.Competitions
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Competition>> GetByStatusAsync(
        CompetitionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Competitions
            .Where(c => c.Status == status)
            .OrderBy(c => c.RegistrationDeadline)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Competition competition, CancellationToken cancellationToken = default)
    {
        await _context.Competitions.AddAsync(competition, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.Competition competition, CancellationToken cancellationToken = default)
    {
        // Entity Framework change tracking automatically detects modifications
        _context.Competitions.Update(competition);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
