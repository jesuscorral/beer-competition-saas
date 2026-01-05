using BeerCompetition.Competition.Domain.Entities;
using FluentValidation;

namespace BeerCompetition.Competition.Application.Features.UserRegistration;

/// <summary>
/// Validator for RegisterUserCommand.
/// Ensures all required fields are present and valid for the specified role.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one digit");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(255)
            .WithMessage("Full name must not exceed 255 characters");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role specified");

        // CompetitionId is required for non-organizers
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .When(x => x.Role != CompetitionUserRole.ORGANIZER)
            .WithMessage("Competition ID is required for this role");

        // BJCP rank validation for judges
        RuleFor(x => x.BjcpRank)
            .Must(rank => rank == null || new[] { "NOVICE", "RECOGNIZED", "CERTIFIED", "NATIONAL", "MASTER", "GRAND_MASTER" }.Contains(rank.ToUpperInvariant()))
            .When(x => x.Role == CompetitionUserRole.JUDGE && !string.IsNullOrEmpty(x.BjcpRank))
            .WithMessage("Invalid BJCP rank. Valid values: NOVICE, RECOGNIZED, CERTIFIED, NATIONAL, MASTER, GRAND_MASTER");
    }
}
