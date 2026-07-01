namespace EnterpriseKit.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// Domain events are raised inside aggregates and stored in
/// the Outbox before being published to MediatR after commit.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this specific event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp when the event occurred.</summary>
    DateTime OccurredOn { get; }
}
