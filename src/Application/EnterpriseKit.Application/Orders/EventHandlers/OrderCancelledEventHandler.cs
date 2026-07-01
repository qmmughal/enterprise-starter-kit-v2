namespace EnterpriseKit.Application.Orders.EventHandlers;

using EnterpriseKit.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>Handles <see cref="OrderCancelledEvent"/> — e.g., trigger refund workflow.</summary>
public sealed class OrderCancelledEventHandler(
    ILogger<OrderCancelledEventHandler> logger)
    : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "OrderCancelled event received — OrderId: {OrderId}, Reason: {Reason}",
            notification.OrderId, notification.Reason);

        // TODO: Initiate refund, release inventory reservations, notify customer
        await Task.CompletedTask;
    }
}
