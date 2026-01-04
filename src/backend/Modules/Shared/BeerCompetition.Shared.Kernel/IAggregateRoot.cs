namespace BeerCompetition.Shared.Kernel;

/// <summary>
/// Marker interface for aggregate roots in Domain-Driven Design (DDD).
/// An aggregate root is the entry point to an aggregate cluster.
/// Only aggregate roots can be directly queried from repositories.
/// All modifications to entities within the aggregate must go through the aggregate root
/// to maintain consistency boundaries.
/// </summary>
/// <remarks>
/// Example: A Flight is an aggregate root that contains FlightEntries.
/// FlightEntries cannot exist independently and must be managed through the Flight aggregate root.
/// </remarks>
public interface IAggregateRoot
{
}
