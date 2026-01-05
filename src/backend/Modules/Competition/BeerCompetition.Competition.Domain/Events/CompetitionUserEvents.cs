using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Events;

/// <summary>
/// Domain event raised when a user registers for a competition.
/// Published for all registration types (Entrant, Judge, Steward).
/// </summary>
public record UserRegisteredForCompetitionEvent(
    Guid CompetitionId,
    string UserId,
    CompetitionUserRole Role,
    CompetitionUserStatus InitialStatus
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when an organizer approves a user's registration.
/// Typically for Judge or Steward approvals.
/// </summary>
public record UserRegistrationApprovedEvent(
    Guid CompetitionId,
    string UserId,
    CompetitionUserRole Role,
    string ApprovedBy
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when an organizer rejects a user's registration.
/// Typically for Judge or Steward rejections.
/// </summary>
public record UserRegistrationRejectedEvent(
    Guid CompetitionId,
    string UserId,
    CompetitionUserRole Role,
    string RejectedBy,
    string? Reason
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
