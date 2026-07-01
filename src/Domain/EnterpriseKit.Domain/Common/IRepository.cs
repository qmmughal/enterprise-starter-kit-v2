namespace EnterpriseKit.Domain.Common;

/// <summary>
/// Generic repository abstraction used by the Application layer.
/// Implementations live in Infrastructure.
/// </summary>
public interface IRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
{
    Task<TEntity?> FindAsync(TId id, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
}
