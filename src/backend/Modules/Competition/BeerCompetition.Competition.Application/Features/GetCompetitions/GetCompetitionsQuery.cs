using BeerCompetition.Shared.Kernel;
using MediatR;

namespace BeerCompetition.Competition.Application.Features.GetCompetitions;

/// <summary>
/// Query to get all competitions for the current tenant.
/// Follows CQRS pattern - read-only operation.
/// </summary>
public record GetCompetitionsQuery : IRequest<Result<List<CompetitionDto>>>;

/// <summary>
/// DTO (Data Transfer Object) representing a competition summary.
/// Used for read operations to avoid exposing domain entities directly.
/// </summary>
public record CompetitionDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime RegistrationDeadline,
    DateTime JudgingStartDate,
    DateTime? JudgingEndDate,
    string Status,
    int MaxEntriesPerEntrant,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
