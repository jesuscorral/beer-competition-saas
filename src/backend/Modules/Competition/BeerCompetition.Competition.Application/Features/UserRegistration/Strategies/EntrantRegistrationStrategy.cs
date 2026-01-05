using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Registration strategy for Entrants.
/// Entrants are auto-approved and can immediately submit entries.
/// </summary>
public class EntrantRegistrationStrategy : IRegistrationStrategy
{
    public CompetitionUserRole Role => CompetitionUserRole.ENTRANT;
    
    public CompetitionUserStatus GetInitialStatus() => CompetitionUserStatus.ACTIVE;
    
    public Task<Result<Guid>> ExecuteRoleSpecificLogic(
        string userId,
        Guid? competitionId,
        Guid? tenantId,
        Dictionary<string, string>? additionalData,
        CancellationToken cancellationToken)
    {
        // Entrants are auto-approved, no additional logic needed
        // Return the provided tenant ID
        return Task.FromResult(Result<Guid>.Success(tenantId!.Value));
    }
}
