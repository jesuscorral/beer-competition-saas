using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Defines the contract for role-specific registration logic.
/// Each role (Entrant, Judge, Steward, Organizer) has its own strategy.
/// </summary>
public interface IRegistrationStrategy
{
    /// <summary>
    /// The role this strategy handles.
    /// </summary>
    CompetitionUserRole Role { get; }
    
    /// <summary>
    /// Gets the initial status for users with this role.
    /// </summary>
    /// <returns>ACTIVE for auto-approved roles, PENDING_APPROVAL for roles requiring approval.</returns>
    CompetitionUserStatus GetInitialStatus();
    
    /// <summary>
    /// Executes role-specific registration logic (e.g., tenant creation for organizers).
    /// </summary>
    /// <param name="userId">Keycloak user ID.</param>
    /// <param name="competitionId">Competition ID (optional, null for organizers).</param>
    /// <param name="tenantId">Tenant ID (will be set by strategy for organizers).</param>
    /// <param name="additionalData">Additional role-specific data (e.g., BJCP rank for judges).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with tenant ID if applicable, or just Success.</returns>
    Task<Result<Guid>> ExecuteRoleSpecificLogic(
        string userId,
        Guid? competitionId,
        Guid? tenantId,
        Dictionary<string, string>? additionalData,
        CancellationToken cancellationToken);
}
