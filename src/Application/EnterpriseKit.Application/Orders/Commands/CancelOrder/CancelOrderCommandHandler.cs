namespace EnterpriseKit.Application.Orders.Commands.CancelOrder;

using EnterpriseKit.Domain.Exceptions;
using EnterpriseKit.Domain.Interfaces.Repositories;
using MediatR;

/// <summary>
/// Handles the Cancel Order command.
///
/// Demonstrates a complex business operation:
///   1. Load aggregate from repository (raises NotFoundException if missing)
///   2. Invoke domain method (raises DomainException on invalid state transition)
///   3. Persist changes — domain events flow to Outbox automatically
/// </summary>
public sealed class CancelOrderCommandHandler(IOrderRepository orders)
    : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.FindAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Domain.Orders.Order), cmd.OrderId);

        // All business rules enforced inside the aggregate
        order.Cancel(cmd.Reason);

        await orders.UpdateAsync(order, ct);
    }
}
