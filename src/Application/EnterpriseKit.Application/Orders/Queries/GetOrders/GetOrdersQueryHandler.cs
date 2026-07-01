namespace EnterpriseKit.Application.Orders.Queries.GetOrders;

using EnterpriseKit.Domain.Interfaces.Repositories;
using MediatR;

public sealed class GetOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(
        GetOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await orders.GetPagedByCustomerAsync(
            query.CustomerId, query.Page, query.PageSize, ct);

        var dtos = items.Select(o => new OrderSummaryDto(
            o.Id,
            o.Status.ToString(),
            o.Total.Amount,
            o.Total.Currency,
            o.Items.Count,
            o.PlacedAt)).ToList();

        return new PagedResult<OrderSummaryDto>(dtos, total, query.Page, query.PageSize);
    }
}
