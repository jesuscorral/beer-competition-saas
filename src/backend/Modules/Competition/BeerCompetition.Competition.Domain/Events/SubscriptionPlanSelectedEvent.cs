using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Events;

/// <summary>
/// Domain event raised when organizer selects subscription plan for competition.
/// MVP: MOCK payment with immediate activation.
/// </summary>
public sealed record SubscriptionPlanSelectedEvent(
    Guid CompetitionId,
    Guid TenantId,
    Guid SubscriptionPlanId,
    int MaxEntries
) : IDomainEvent;
