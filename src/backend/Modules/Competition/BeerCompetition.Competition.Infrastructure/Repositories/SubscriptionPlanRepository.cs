using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeerCompetition.Competition.Infrastructure.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly CompetitionDbContext _dbContext;

    public SubscriptionPlanRepository(CompetitionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Global filter auto-applies tenant isolation
        return await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Global filter auto-applies tenant isolation
        return await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<List<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Global filter auto-applies tenant isolation
        return await _dbContext.SubscriptionPlans
            .OrderBy(p => p.PriceAmount)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default)
    {
        await _dbContext.SubscriptionPlans.AddAsync(subscriptionPlan, cancellationToken);
    }

    public async Task UpdateAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default)
    {
        _dbContext.SubscriptionPlans.Update(subscriptionPlan);
    }
}
