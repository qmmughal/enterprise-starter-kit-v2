namespace EnterpriseKit.Domain.Common;

/// <summary>
/// Marks the root of a consistency boundary (aggregate).
/// Only aggregate roots are referenced by repositories and
/// can have their domain events dispatched.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    /// <summary>
    /// Optimistic concurrency version token. Incremented on every
    /// business operation that mutates state. Maps to Postgres xmin
    /// or a dedicated row version column.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Called at the end of every state-mutating operation so that
    /// the persistence layer can detect concurrent modifications.
    /// </summary>
    protected void IncrementVersion() => Version++;
}
