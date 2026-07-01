namespace EnterpriseKit.Application.Orders.Queries.GetOrderById;

using EnterpriseKit.Domain.Exceptions;
using EnterpriseKit.Domain.Interfaces.Repositories;
using MediatR;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await orders.FindAsync(query.OrderId, ct)
            ?? throw new NotFoundException(nameof(Domain.Orders.Order), query.OrderId);

        return new OrderDetailDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Total.Amount,
            order.Total.Currency,
            order.PlacedAt,
            order.CancellationReason,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.Quantity,
                i.UnitPrice.Amount,
                i.LineTotal.Amount,
                i.UnitPrice.Currency)).ToList());
    }
}
