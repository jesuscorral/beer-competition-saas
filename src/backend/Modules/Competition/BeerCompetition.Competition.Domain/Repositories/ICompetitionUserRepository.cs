using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.Domain.Repositories;

/// <summary>
/// Repository interface for CompetitionUser aggregate root.
/// Follows Repository pattern from Domain-Driven Design.
/// Implementation resides in Infrastructure layer.
/// </summary>
public interface ICompetitionUserRepository
{
    /// <summary>
    /// Gets a competition user registration by ID.
    /// Returns null if not found or user has no access (multi-tenancy).
    /// </summary>
    Task<CompetitionUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's registration for a specific competition and role.
    /// Returns null if no such registration exists.
    /// </summary>
    /// <param name="competitionId">Competition ID.</param>
    /// <param name="userId">Keycloak user ID.</param>
    /// <param name="role">User role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CompetitionUser?> GetByCompetitionUserAndRoleAsync(
        Guid competitionId, 
        string userId, 
        CompetitionUserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registrations for a specific competition.
    /// Filtered by tenant_id automatically via Entity Framework global query filter.
    /// </summary>
    Task<List<CompetitionUser>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registrations for a specific user across competitions.
    /// Filtered by tenant_id automatically via Entity Framework global query filter.
    /// </summary>
    Task<List<CompetitionUser>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending approval registrations for a competition (for organizer review).
    /// </summary>
    Task<List<CompetitionUser>> GetPendingApprovalsByCompetitionIdAsync(
        Guid competitionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is already registered for a competition with a specific role.
    /// Used to prevent duplicate registrations.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid competitionId, 
        string userId, 
        CompetitionUserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new competition user registration to the repository.
    /// Changes are not persisted until SaveChangesAsync is called.
    /// </summary>
    Task AddAsync(CompetitionUser competitionUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing competition user registration.
    /// Entity Framework change tracking automatically detects modifications.
    /// </summary>
    Task UpdateAsync(CompetitionUser competitionUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database.
    /// Should be called within a Unit of Work transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
