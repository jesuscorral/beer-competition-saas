using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.Domain.Repositories;

/// <summary>
/// Repository interface for Competition aggregate root.
/// Follows Repository pattern from Domain-Driven Design.
/// Implementation resides in Infrastructure layer.
/// </summary>
public interface ICompetitionRepository
{
    /// <summary>
    /// Gets a competition by its ID.
    /// Returns null if competition not found or user has no access (multi-tenancy).
    /// </summary>
    Task<Entities.Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all competitions for the current tenant.
    /// Filtered by tenant_id automatically via Entity Framework global query filter.
    /// </summary>
    Task<List<Entities.Competition>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets competitions by status for the current tenant.
    /// </summary>
    Task<List<Entities.Competition>> GetByStatusAsync(CompetitionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new competition to the repository.
    /// Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    Task AddAsync(Entities.Competition competition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing competition.
    /// Entity Framework change tracking automatically detects modifications.
    /// </summary>
    Task UpdateAsync(Entities.Competition competition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database.
    /// Should be called within a Unit of Work transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
