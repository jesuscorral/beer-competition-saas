using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeerCompetition.Competition.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CompetitionUser aggregate.
/// Delegates persistence logic to Entity Framework Core DbContext.
/// </summary>
public class CompetitionUserRepository : ICompetitionUserRepository
{
    private readonly CompetitionDbContext _context;

    public CompetitionUserRepository(CompetitionDbContext context)
    {
        _context = context;
    }

    public async Task<CompetitionUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically adds tenant_id filter
        return await _context.CompetitionUsers
            .FirstOrDefaultAsync(cu => cu.Id == id, cancellationToken);
    }

    public async Task<CompetitionUser?> GetByCompetitionUserAndRoleAsync(
        Guid competitionId, 
        string userId, 
        CompetitionUserRole role,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionUsers
            .FirstOrDefaultAsync(cu => 
                cu.CompetitionId == competitionId && 
                cu.UserId == userId && 
                cu.Role == role, 
                cancellationToken);
    }

    public async Task<List<CompetitionUser>> GetByCompetitionIdAsync(
        Guid competitionId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionUsers
            .Where(cu => cu.CompetitionId == competitionId)
            .OrderByDescending(cu => cu.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CompetitionUser>> GetByUserIdAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionUsers
            .Where(cu => cu.UserId == userId)
            .OrderByDescending(cu => cu.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CompetitionUser>> GetPendingApprovalsByCompetitionIdAsync(
        Guid competitionId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionUsers
            .Where(cu => cu.CompetitionId == competitionId && cu.Status == CompetitionUserStatus.PENDING_APPROVAL)
            .OrderBy(cu => cu.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid competitionId, 
        string userId, 
        CompetitionUserRole role,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionUsers
            .AnyAsync(cu => 
                cu.CompetitionId == competitionId && 
                cu.UserId == userId && 
                cu.Role == role, 
                cancellationToken);
    }

    public async Task AddAsync(CompetitionUser competitionUser, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionUsers.AddAsync(competitionUser, cancellationToken);
    }

    public Task UpdateAsync(CompetitionUser competitionUser, CancellationToken cancellationToken = default)
    {
        // Entity Framework change tracking automatically detects modifications
        _context.CompetitionUsers.Update(competitionUser);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
