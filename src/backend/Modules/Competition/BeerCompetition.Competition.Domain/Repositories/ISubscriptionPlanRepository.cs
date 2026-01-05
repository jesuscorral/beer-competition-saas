namespace BeerCompetition.Competition.Domain.Repositories;

public interface ISubscriptionPlanRepository
{
    Task<Entities.SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Entities.SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Entities.SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entities.SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default);
}
