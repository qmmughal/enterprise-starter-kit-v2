namespace EnterpriseKit.Domain.Interfaces.Repositories;

using EnterpriseKit.Domain.Orders;

/// <summary>
/// Repository contract for the <see cref="Order"/> aggregate.
/// Concrete implementation lives in Infrastructure.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Finds an order by its ID, including all items. Returns null if not found.</summary>
    Task<Order?> FindAsync(Guid id, CancellationToken ct = default);

    /// <summary>Retrieves a paged list of orders for a specific customer.</summary>
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Persists a new order.</summary>
    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>Marks an existing order as modified.</summary>
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
