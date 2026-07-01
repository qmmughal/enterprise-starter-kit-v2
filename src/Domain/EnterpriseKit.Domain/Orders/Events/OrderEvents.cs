namespace EnterpriseKit.Domain.Orders.Events;

using EnterpriseKit.Domain.Common;
using MediatR;

/// <summary>Raised when a new order is successfully placed.</summary>
public sealed record OrderPlacedEvent(Guid OrderId, Guid CustomerId) : IDomainEvent, INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an order is confirmed (e.g., payment received).</summary>
public sealed record OrderConfirmedEvent(Guid OrderId) : IDomainEvent, INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an order is cancelled.</summary>
public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent, INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
