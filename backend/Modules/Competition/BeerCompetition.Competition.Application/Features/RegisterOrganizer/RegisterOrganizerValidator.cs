using FluentValidation;

namespace BeerCompetition.Competition.Application.Features.RegisterOrganizer;

/// <summary>
/// Validator for RegisterOrganizerCommand.
/// Ensures all required fields meet business rules before processing.
/// </summary>
public class RegisterOrganizerValidator : AbstractValidator<RegisterOrganizerCommand>
{
    public RegisterOrganizerValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required")
            .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters");

        RuleFor(x => x.CompetitionName)
            .NotEmpty().WithMessage("Competition name is required")
            .MaximumLength(255).WithMessage("Competition name must not exceed 255 characters");

        RuleFor(x => x.PlanName)
            .NotEmpty().WithMessage("Plan name is required")
            .Must(BeValidPlanName).WithMessage("Plan name must be one of: TRIAL, BASIC, STANDARD, PRO");
    }

    private bool BeValidPlanName(string planName)
    {
        var validPlans = new[] { "TRIAL", "BASIC", "STANDARD", "PRO" };
        return validPlans.Contains(planName?.ToUpperInvariant());
    }
}
