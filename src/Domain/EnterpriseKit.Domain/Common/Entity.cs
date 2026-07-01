namespace EnterpriseKit.Domain.Common;

/// <summary>
/// Base class for all domain entities. Carries an identity key and a
/// collection of domain events that are raised during aggregate mutation.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; } = default!;

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised since the last <see cref="ClearDomainEvents"/> call.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Registers a new domain event to be dispatched after persistence.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>Clears all pending domain events (called by the Outbox extension after serialisation).</summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    // ── Equality ──────────────────────────────────────────────────────────
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
