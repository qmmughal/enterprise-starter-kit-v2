namespace EnterpriseKit.Infrastructure.Persistence;

using EnterpriseKit.Domain.Orders;
using EnterpriseKit.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// The application's primary EF Core DbContext.
///
/// Key responsibilities:
///   • Owns all entity DbSets
///   • Applies fluent configurations from the assembly
///   • Drains domain events to the Outbox on every save
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-discover all IEntityTypeConfiguration<T> in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Default schema
        builder.HasDefaultSchema("app");
    }

    /// <summary>
    /// Overridden to drain domain events into the Outbox table
    /// BEFORE the underlying commit — ensuring atomicity.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        this.DispatchDomainEventsToOutbox();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.DispatchDomainEventsToOutbox();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
}
