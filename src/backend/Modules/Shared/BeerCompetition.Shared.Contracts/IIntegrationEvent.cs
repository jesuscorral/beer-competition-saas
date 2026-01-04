namespace BeerCompetition.Shared.Contracts;

/// <summary>
/// Marker interface for integration events.
/// Integration events are used for communication between bounded contexts (modules).
/// Unlike domain events (in-process), integration events are published to RabbitMQ
/// for asynchronous cross-module communication.
/// </summary>
/// <remarks>
/// Integration events follow CloudEvents 1.0 specification for standardized event format.
/// 
/// Key differences from Domain Events:
/// - Domain Events: In-process (MediatR), immediate consistency, same transaction
/// - Integration Events: Cross-process (RabbitMQ), eventual consistency, separate transactions
/// 
/// Publishing pattern:
/// 1. Domain event triggers integration event creation
/// 2. Integration event stored in Outbox table (same transaction as domain change)
/// 3. Background worker reads Outbox and publishes to RabbitMQ
/// 4. Consumer modules subscribe to RabbitMQ topics
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// Used for idempotency checks in consumers.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Tenant identifier for multi-tenancy isolation.
    /// Consumers can filter events by tenant.
    /// </summary>
    Guid TenantId { get; }
}
