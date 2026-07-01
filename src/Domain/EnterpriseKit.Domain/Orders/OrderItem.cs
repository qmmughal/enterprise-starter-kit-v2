namespace EnterpriseKit.Domain.Orders;

using EnterpriseKit.Domain.Common;

/// <summary>
/// A line item within an <see cref="Order"/>.
/// OrderItem is an Entity (has identity within the aggregate) but is NOT
/// an AggregateRoot — it can only be reached through the Order.
/// </summary>
public sealed class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;

    /// <summary>Computed line total: Quantity × UnitPrice.</summary>
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    // EF Core constructor
    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Guid productId, int quantity, Money unitPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentNullException.ThrowIfNull(unitPrice);

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    /// <summary>Updates quantity — only the owning Order should call this.</summary>
    internal void ChangeQuantity(int newQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newQuantity);
        Quantity = newQuantity;
    }
}
