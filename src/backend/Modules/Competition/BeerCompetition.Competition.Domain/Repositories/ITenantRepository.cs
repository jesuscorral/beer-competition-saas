using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.Domain.Repositories;

/// <summary>
/// Repository interface for Tenant aggregate root.
/// Handles persistence operations for tenant (organization) accounts.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Gets a tenant by its ID.
    /// </summary>
    /// <param name="id">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tenant entity or null if not found.</returns>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by email address.
    /// Useful for checking if an organization already exists.
    /// </summary>
    /// <param name="email">The organization's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tenant entity or null if not found.</returns>
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new tenant to the repository.
    /// Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    /// <param name="tenant">The tenant entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tenant.
    /// Entity Framework change tracking automatically detects modifications.
    /// </summary>
    /// <param name="tenant">The tenant entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database.
    /// Should be called within a Unit of Work transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entities written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
