namespace EnterpriseKit.Application.Orders.Queries.GetOrderById;

/// <summary>Full order detail DTO returned to the API consumer.</summary>
public sealed record OrderDetailDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset PlacedAt,
    string? CancellationReason,
    IReadOnlyList<OrderItemDto> Items);

/// <summary>A single line item within an order detail response.</summary>
public sealed record OrderItemDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Currency);
