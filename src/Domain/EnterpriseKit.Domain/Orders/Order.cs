namespace EnterpriseKit.Domain.Orders;

using EnterpriseKit.Domain.Common;
using EnterpriseKit.Domain.Exceptions;
using EnterpriseKit.Domain.Orders.Events;

/// <summary>
/// The Order Aggregate Root. Represents a customer's purchase order.
///
/// All state mutations MUST go through methods on this class.
/// The constructor is private — use the <see cref="Place"/> factory method.
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; } = null!;
    public DateTimeOffset PlacedAt { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <summary>Read-only view of line items.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Order() { }

    // ── Factory Method ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates and validates a new order. Raises <see cref="OrderPlacedEvent"/>.
    /// </summary>
    /// <param name="customerId">The customer placing the order.</param>
    /// <param name="lines">Product lines: (ProductId, Quantity, UnitPrice).</param>
    /// <param name="currency">ISO 4217 currency code for all line items.</param>
    public static Order Place(
        Guid customerId,
        IEnumerable<(Guid ProductId, int Quantity, decimal UnitPrice)> lines,
        string currency = "USD")
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0)
            throw new DomainException("ORDER_EMPTY", "An order must contain at least one item.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            PlacedAt = DateTimeOffset.UtcNow
        };

        foreach (var (productId, qty, unitPrice) in lineList)
            order._items.Add(OrderItem.Create(order.Id, productId, qty, Money.Of(unitPrice, currency)));

        order.Total = order.CalculateTotal();
        order.RaiseDomainEvent(new OrderPlacedEvent(order.Id, order.CustomerId));
        order.IncrementVersion();

        return order;
    }

    // ── Business Operations ────────────────────────────────────────────────

    /// <summary>Confirms the order (e.g., after payment gateway callback).</summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("ORDER_NOT_PENDING", $"Cannot confirm an order in '{Status}' status.");

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(Id));
        IncrementVersion();
    }

    /// <summary>Marks the order as shipped by logistics.</summary>
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("ORDER_NOT_CONFIRMED", $"Cannot ship an order in '{Status}' status.");

        Status = OrderStatus.Shipped;
        IncrementVersion();
    }

    /// <summary>Cancels the order. Cannot cancel a shipped order.</summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new DomainException(
                "ORDER_CANNOT_CANCEL",
                $"Orders in '{Status}' status cannot be cancelled.");

        if (Status == OrderStatus.Cancelled)
            throw new DomainException("ORDER_ALREADY_CANCELLED", "The order is already cancelled.");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason ?? throw new ArgumentNullException(nameof(reason));
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
        IncrementVersion();
    }

    /// <summary>Updates the quantity of an existing line item.</summary>
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("ORDER_NOT_EDITABLE", "Only pending orders can be modified.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException(nameof(OrderItem), productId);

        item.ChangeQuantity(newQuantity);
        Total = CalculateTotal();
        IncrementVersion();
    }

    // ── Private Helpers ────────────────────────────────────────────────────

    private Money CalculateTotal()
        => _items.Aggregate(
            Money.Zero(_items.First().UnitPrice.Currency),
            (acc, item) => acc.Add(item.LineTotal));
}
