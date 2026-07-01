namespace EnterpriseKit.Domain.Orders;

/// <summary>Lifecycle states of an <see cref="Order"/>.</summary>
public enum OrderStatus
{
    /// <summary>Created but not yet confirmed.</summary>
    Pending = 0,

    /// <summary>Payment confirmed, awaiting fulfilment.</summary>
    Confirmed = 1,

    /// <summary>Order handed to logistics.</summary>
    Shipped = 2,

    /// <summary>Order delivered to customer.</summary>
    Delivered = 3,

    /// <summary>Order cancelled before shipping.</summary>
    Cancelled = 4
}
