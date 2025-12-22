namespace BeerCompetition.Shared.Kernel;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent state changes in the domain that other parts of the system
/// may need to react to.
/// </summary>
/// <remarks>
/// Domain events follow these principles:
/// 1. **In-Process Communication**: Published via MediatR within the same process
/// 2. **Eventual Consistency**: Handlers execute asynchronously after entity is persisted
/// 3. **Transactional Integrity**: Events stored in Outbox pattern for reliable publishing
/// 4. **Immutable**: Events are records representing facts that already happened
/// 
/// Example: CompetitionCreatedEvent, EntrySubmittedEvent, ScoresheetCompletedEvent
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId => Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    DateTime OccurredOn => DateTime.UtcNow;
}
