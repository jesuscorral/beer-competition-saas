using FluentValidation;

namespace BeerCompetition.Competition.Application.Features.CreateCompetition;

/// <summary>
/// Validator for CreateCompetitionCommand.
/// Executes before handler via MediatR pipeline behavior.
/// Validates input format and business rules at application layer.
/// </summary>
public class CreateCompetitionValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Competition name is required")
            .MaximumLength(255).WithMessage("Competition name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.RegistrationDeadline)
            .GreaterThan(DateTime.UtcNow).WithMessage("Registration deadline must be in the future");

        RuleFor(x => x.JudgingStartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Judging start date must be in the future")
            .GreaterThan(x => x.RegistrationDeadline).WithMessage("Judging start date must be after registration deadline");

        RuleFor(x => x.JudgingEndDate)
            .GreaterThan(x => x.JudgingStartDate).WithMessage("Judging end date must be after judging start date")
            .When(x => x.JudgingEndDate.HasValue);

        RuleFor(x => x.MaxEntriesPerEntrant)
            .GreaterThan(0).WithMessage("Max entries per entrant must be at least 1")
            .LessThanOrEqualTo(100).WithMessage("Max entries per entrant cannot exceed 100");

    }
}
