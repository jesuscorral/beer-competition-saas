using MediatR;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.SelectPlan;

/// <summary>
/// Command to select a subscription plan for a competition.
/// MVP: Uses MOCK payment with immediate activation.
/// </summary>
/// <param name="CompetitionId">ID of the competition to assign plan to.</param>
/// <param name="PlanName">Name of the plan to select (TRIAL, BASIC, STANDARD, PRO).</param>
public record SelectPlanCommand(
    Guid CompetitionId,
    string PlanName
) : IRequest<Result<SelectPlanResponse>>;

/// <summary>
/// Response after selecting a subscription plan.
/// </summary>
public record SelectPlanResponse
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public int MaxEntries { get; init; }
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = "MOCK_PAID";
    public bool IsPublic { get; init; }
}
