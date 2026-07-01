namespace EnterpriseKit.Application.Orders.Commands.PlaceOrder;

using EnterpriseKit.Domain.Interfaces.Repositories;
using EnterpriseKit.Domain.Orders;
using MediatR;

/// <summary>
/// Handles <see cref="PlaceOrderCommand"/>.
///
/// This handler is intentionally thin — all business logic lives
/// inside <see cref="Order.Place"/>. The handler only orchestrates:
///   1. Map command → domain call
///   2. Persist via repository
///   3. Return the new aggregate ID
///
/// Domain events are drained to the Outbox by <c>ApplicationDbContext.SaveChangesAsync</c>.
/// </summary>
public sealed class PlaceOrderCommandHandler(IOrderRepository orders)
    : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        // Derive currency from the first line (validator guarantees all are the same)
        var currency = cmd.Lines[0].Currency;

        var lines = cmd.Lines.Select(l => (l.ProductId, l.Quantity, l.UnitPrice));

        var order = Order.Place(cmd.CustomerId, lines, currency);

        await orders.AddAsync(order, ct);

        return order.Id;
    }
}
