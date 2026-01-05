using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Registration strategy for Judges.
/// Judges require organizer approval before accessing judging features.
/// </summary>
public class JudgeRegistrationStrategy : IRegistrationStrategy
{
    public CompetitionUserRole Role => CompetitionUserRole.JUDGE;
    
    public CompetitionUserStatus GetInitialStatus() => CompetitionUserStatus.PENDING_APPROVAL;
    
    public Task<Result<Guid>> ExecuteRoleSpecificLogic(
        string userId,
        Guid? competitionId,
        Guid? tenantId,
        Dictionary<string, string>? additionalData,
        CancellationToken cancellationToken)
    {
        // Judges require approval, no additional logic needed
        // BJCP rank is handled separately via Keycloak attributes
        return Task.FromResult(Result<Guid>.Success(tenantId!.Value));
    }
}
