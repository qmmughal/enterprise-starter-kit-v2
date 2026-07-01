namespace EnterpriseKit.Application.Orders.Commands.PlaceOrder;

using EnterpriseKit.Application.Common.Interfaces;

/// <summary>
/// Command: Place a new customer order.
/// Returns the newly created Order ID on success.
/// </summary>
public sealed record PlaceOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderLineInput> Lines) : ICommand<Guid>;

/// <summary>A single product line within the place-order request.</summary>
public sealed record OrderLineInput(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string Currency = "USD");
