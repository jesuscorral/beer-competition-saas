using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Registration strategy for Stewards.
/// Stewards require organizer approval before accessing steward features.
/// </summary>
public class StewardRegistrationStrategy : IRegistrationStrategy
{
    public CompetitionUserRole Role => CompetitionUserRole.STEWARD;
    
    public CompetitionUserStatus GetInitialStatus() => CompetitionUserStatus.PENDING_APPROVAL;
    
    public Task<Result<Guid>> ExecuteRoleSpecificLogic(
        string userId,
        Guid? competitionId,
        Guid? tenantId,
        Dictionary<string, string>? additionalData,
        CancellationToken cancellationToken)
    {
        // Stewards require approval, no additional logic needed
        return Task.FromResult(Result<Guid>.Success(tenantId!.Value));
    }
}
