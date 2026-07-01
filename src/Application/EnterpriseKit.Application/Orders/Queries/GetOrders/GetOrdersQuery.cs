namespace EnterpriseKit.Application.Orders.Queries.GetOrders;

using EnterpriseKit.Application.Common.Interfaces;

/// <summary>Query: Returns a paginated list of orders for a specific customer.</summary>
public sealed record GetOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<OrderSummaryDto>>;

/// <summary>Lightweight order summary for list views.</summary>
public sealed record OrderSummaryDto(
    Guid Id,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTimeOffset PlacedAt);

/// <summary>Generic paged result wrapper.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
