using FluentValidation;

namespace BeerCompetition.Competition.Application.Features.SelectPlan;

/// <summary>
/// Validator for SelectPlanCommand.
/// Ensures valid competition ID and plan name before processing.
/// </summary>
public class SelectPlanCommandValidator : AbstractValidator<SelectPlanCommand>
{
    private static readonly string[] ValidPlanNames = { "TRIAL", "BASIC", "STANDARD", "PRO" };

    public SelectPlanCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithMessage("Competition ID is required");

        RuleFor(x => x.PlanName)
            .NotEmpty()
            .WithMessage("Plan name is required")
            .Must(BeValidPlanName)
            .WithMessage($"Invalid plan name. Valid options: {string.Join(", ", ValidPlanNames)}");
    }

    private bool BeValidPlanName(string planName)
    {
        return !string.IsNullOrWhiteSpace(planName) && 
               ValidPlanNames.Contains(planName.ToUpperInvariant());
    }
}
