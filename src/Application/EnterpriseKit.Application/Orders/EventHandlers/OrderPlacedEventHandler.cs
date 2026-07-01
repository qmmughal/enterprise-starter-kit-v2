namespace EnterpriseKit.Application.Orders.EventHandlers;

using EnterpriseKit.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles <see cref="OrderPlacedEvent"/> dispatched by the Outbox relay.
///
/// NOTE: This handler runs AFTER the database transaction is committed,
/// ensuring the order is safely persisted before any side effects occur.
/// </summary>
public sealed class OrderPlacedEventHandler(
    ILogger<OrderPlacedEventHandler> logger)
    : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "OrderPlaced event received — OrderId: {OrderId}, CustomerId: {CustomerId}",
            notification.OrderId, notification.CustomerId);

        // TODO: Integrate real use-cases here, e.g.:
        //   - Send confirmation email via IEmailService
        //   - Publish to message broker (RabbitMQ / Azure Service Bus)
        //   - Trigger inventory reservation via IInventoryService

        await Task.CompletedTask;
    }
}
