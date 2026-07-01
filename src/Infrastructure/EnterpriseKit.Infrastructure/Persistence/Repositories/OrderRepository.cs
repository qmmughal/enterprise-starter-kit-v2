namespace EnterpriseKit.Infrastructure.Persistence.Repositories;

using EnterpriseKit.Domain.Interfaces.Repositories;
using EnterpriseKit.Domain.Orders;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core implementation of <see cref="IOrderRepository"/>.
/// Lives in Infrastructure — the Domain only knows the interface.
/// </summary>
public sealed class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    /// <inheritdoc />
    public async Task<Order?> FindAsync(Guid id, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PlacedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <inheritdoc />
    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await db.Orders.AddAsync(order, ct);

    /// <inheritdoc />
    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Update(order);
        return Task.CompletedTask;
    }
}
