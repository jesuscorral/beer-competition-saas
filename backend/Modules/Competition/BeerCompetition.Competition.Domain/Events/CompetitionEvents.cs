using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Events;

/// <summary>
/// Domain event raised when a new competition is created.
/// Other modules (e.g., Judging) may listen to prepare for flight creation.
/// </summary>
public record CompetitionCreatedEvent(
    Guid CompetitionId,
    Guid TenantId,
    string Name
) : IDomainEvent;

/// <summary>
/// Domain event raised when a competition is opened for registration.
/// Triggers notification to potential entrants.
/// </summary>
public record CompetitionOpenedEvent(
    Guid CompetitionId,
    Guid TenantId
) : IDomainEvent;

/// <summary>
/// Domain event raised when judging starts for a competition.
/// Signals that no more entries can be submitted.
/// </summary>
public record JudgingStartedEvent(
    Guid CompetitionId,
    Guid TenantId
) : IDomainEvent;

/// <summary>
/// Domain event raised when judging is completed.
/// Triggers final score aggregation and placement calculation.
/// </summary>
public record JudgingCompletedEvent(
    Guid CompetitionId,
    Guid TenantId
) : IDomainEvent;

/// <summary>
/// Domain event raised when competition results are published.
/// Triggers notification to all entrants with their scoresheets.
/// </summary>
public record ResultsPublishedEvent(
    Guid CompetitionId,
    Guid TenantId
) : IDomainEvent;

/// <summary>
/// Domain event raised when a competition is canceled.
/// Triggers refund processing and notifications.
/// </summary>
public record CompetitionCanceledEvent(
    Guid CompetitionId,
    Guid TenantId
) : IDomainEvent;
