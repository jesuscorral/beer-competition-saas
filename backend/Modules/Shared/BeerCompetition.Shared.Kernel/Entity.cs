namespace BeerCompetition.Shared.Kernel;

/// <summary>
/// Base class for all domain entities following Domain-Driven Design (DDD) principles.
/// Entities have identity (Id) and lifecycle tracking (CreatedAt, UpdatedAt).
/// Supports multi-tenancy via ITenantEntity interface.
/// </summary>
public abstract class Entity : ITenantEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// Generated automatically upon creation.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenancy isolation.
    /// All entities must belong to a tenant.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated (UTC).
    /// Null if entity has never been updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Collection of domain events raised by this entity.
    /// Domain events represent state changes that other parts of the system may need to react to.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only access to domain events.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// Events are published after the entity is persisted to ensure consistency.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// Called after events have been published to avoid duplicate processing.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to the current UTC time.
    /// Should be called when modifying entity properties.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Entities are considered equal if they have the same Id and are of the same type.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Returns hash code based on entity Id.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b)
    {
        return !(a == b);
    }
}
