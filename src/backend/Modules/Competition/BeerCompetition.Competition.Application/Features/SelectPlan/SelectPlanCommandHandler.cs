using MediatR;
using Microsoft.Extensions.Logging;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.SelectPlan;

/// <summary>
/// Handler for selecting a subscription plan for a competition.
/// MVP: MOCK payment with immediate activation and public visibility.
/// </summary>
public class SelectPlanCommandHandler : IRequestHandler<SelectPlanCommand, Result<SelectPlanResponse>>
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<SelectPlanCommandHandler> _logger;

    public SelectPlanCommandHandler(
        ICompetitionRepository competitionRepository,
        ISubscriptionPlanRepository planRepository,
        ILogger<SelectPlanCommandHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<Result<SelectPlanResponse>> Handle(
        SelectPlanCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Selecting plan {PlanName} for competition {CompetitionId}",
                request.PlanName, request.CompetitionId);

            // 1. Get competition
            var competition = await _competitionRepository.GetByIdAsync(
                request.CompetitionId, cancellationToken);

            if (competition == null)
            {
                _logger.LogWarning("Competition {CompetitionId} not found", request.CompetitionId);
                return Result<SelectPlanResponse>.Failure("Competition not found");
            }

            // 2. Check if competition already has a plan
            if (competition.SubscriptionPlanId.HasValue)
            {
                _logger.LogWarning(
                    "Competition {CompetitionId} already has a subscription plan",
                    request.CompetitionId);
                return Result<SelectPlanResponse>.Failure(
                    "Competition already has a subscription plan selected");
            }

            // 3. Get plan by name
            var plan = await _planRepository.GetByNameAsync(
                request.PlanName.ToUpperInvariant(), cancellationToken);

            if (plan == null)
            {
                _logger.LogWarning("Subscription plan {PlanName} not found", request.PlanName);
                return Result<SelectPlanResponse>.Failure($"Plan '{request.PlanName}' not found");
            }

            // 4. Verify tenant match (security check)
            if (plan.TenantId != competition.TenantId)
            {
                _logger.LogError(
                    "Tenant mismatch: Plan {PlanId} belongs to tenant {PlanTenantId}, " +
                    "but competition {CompetitionId} belongs to tenant {CompetitionTenantId}",
                    plan.Id, plan.TenantId, competition.Id, competition.TenantId);
                return Result<SelectPlanResponse>.Failure(
                    "Invalid plan selection: tenant mismatch");
            }

            // 5. Set plan on competition (domain logic)
            var setResult = competition.SetSubscriptionPlan(plan);
            if (setResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to set plan on competition {CompetitionId}: {Error}",
                    request.CompetitionId, setResult.Error);
                return Result<SelectPlanResponse>.Failure(setResult.Error);
            }

            // 6. Persist changes
            await _competitionRepository.UpdateAsync(competition, cancellationToken);
            await _competitionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully selected plan {PlanName} (ID: {PlanId}) for competition {CompetitionId}. " +
                "Max entries: {MaxEntries}, Payment status: MOCK_PAID",
                plan.Name, plan.Id, competition.Id, plan.MaxEntries);

            // 7. Return response
            return Result<SelectPlanResponse>.Success(new SelectPlanResponse
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                MaxEntries = plan.MaxEntries,
                PriceAmount = plan.PriceAmount,
                PriceCurrency = plan.PriceCurrency,
                PaymentStatus = "MOCK_PAID",
                IsPublic = competition.IsPublic
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error selecting plan {PlanName} for competition {CompetitionId}",
                request.PlanName, request.CompetitionId);
            return Result<SelectPlanResponse>.Failure(
                "An error occurred while selecting the subscription plan");
        }
    }
}
