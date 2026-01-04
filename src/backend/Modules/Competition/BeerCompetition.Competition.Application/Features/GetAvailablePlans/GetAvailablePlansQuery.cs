using MediatR;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.GetAvailablePlans;

/// <summary>
/// Query to get all available subscription plans for the current tenant.
/// Returns plan details including pricing and entry limits.
/// </summary>
public record GetAvailablePlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>;

/// <summary>
/// DTO representing a subscription plan with pricing and limits.
/// </summary>
public record SubscriptionPlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MaxEntries { get; init; }
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsRecommended { get; init; }
}
