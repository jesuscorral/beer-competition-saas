using MediatR;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Application.Features.CreateCompetition;

/// <summary>
/// Command to create a new competition.
/// Follows CQRS pattern - commands change state, queries return data.
/// </summary>
/// <param name="Name">Competition name (required, max 255 characters).</param>
/// <param name="Description">Optional description with competition details.</param>
/// <param name="RegistrationDeadline">Last date for entry submission (UTC).</param>
/// <param name="JudgingStartDate">First day of judging (UTC).</param>
/// <param name="JudgingEndDate">Optional last day of judging (UTC).</param>
/// <param name="MaxEntriesPerEntrant">Maximum entries allowed per person (default: 10).</param>
/// <returns>Result with Competition ID or error message.</returns>
public record CreateCompetitionCommand(
    string Name,
    string? Description,
    DateTime RegistrationDeadline,
    DateTime JudgingStartDate,
    DateTime? JudgingEndDate,
    int MaxEntriesPerEntrant = 10
) : IRequest<Result<Guid>>;
