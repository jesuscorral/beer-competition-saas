using MediatR;
using Microsoft.Extensions.Logging;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.GetAvailablePlans;

/// <summary>
/// Handler for retrieving all available subscription plans.
/// Returns plans filtered by current tenant (multi-tenancy enforced by repository).
/// </summary>
public class GetAvailablePlansQueryHandler : IRequestHandler<GetAvailablePlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<GetAvailablePlansQueryHandler> _logger;

    public GetAvailablePlansQueryHandler(
        ISubscriptionPlanRepository planRepository,
        ILogger<GetAvailablePlansQueryHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(
        GetAvailablePlansQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving available subscription plans");

            var plans = await _planRepository.GetAllAsync(cancellationToken);

            if (plans == null || !plans.Any())
            {
                _logger.LogWarning("No subscription plans found for current tenant");
                return Result<List<SubscriptionPlanDto>>.Success(new List<SubscriptionPlanDto>());
            }

            var planDtos = plans.Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                MaxEntries = p.MaxEntries,
                PriceAmount = p.PriceAmount,
                PriceCurrency = p.PriceCurrency,
                Description = GetPlanDescription(p.Name),
                IsRecommended = p.Name == "BASIC" // BASIC is recommended for most organizers
            }).OrderBy(p => p.PriceAmount).ToList();

            _logger.LogInformation("Retrieved {PlanCount} subscription plans", planDtos.Count);

            return Result<List<SubscriptionPlanDto>>.Success(planDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plans");
            return Result<List<SubscriptionPlanDto>>.Failure("Failed to retrieve subscription plans");
        }
    }

    private static string GetPlanDescription(string planName) => planName switch
    {
        "TRIAL" => "Perfect for small homebrew club competitions with up to 10 entries",
        "BASIC" => "Ideal for local competitions with up to 50 entries",
        "STANDARD" => "Great for regional competitions with up to 200 entries",
        "PRO" => "Best for large national competitions with up to 600 entries",
        _ => "Subscription plan for beer competitions"
    };
}
