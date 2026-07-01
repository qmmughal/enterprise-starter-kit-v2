namespace EnterpriseKit.Infrastructure.Persistence;

using System.Text.Json;
using EnterpriseKit.Domain.Common;
using EnterpriseKit.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods to drain domain events from tracked aggregates
/// into OutboxMessage rows — called inside SaveChangesAsync before the commit.
/// </summary>
public static class OutboxExtensions
{
    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Scans all tracked <see cref="Entity{Guid}"/> instances for pending
    /// domain events and converts them to <see cref="OutboxMessage"/> rows.
    /// </summary>
    public static void DispatchDomainEventsToOutbox(this ApplicationDbContext dbContext)
    {
        var entitiesWithEvents = dbContext.ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        var outboxMessages = entitiesWithEvents
            .SelectMany(entry => entry.Entity.DomainEvents.Select(evt => new OutboxMessage
            {
                Type = evt.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(evt, evt.GetType(), JsonOpts)
            }))
            .ToList();

        // Clear events BEFORE adding to outbox to avoid double-dispatch on retry
        entitiesWithEvents.ForEach(e => e.Entity.ClearDomainEvents());

        if (outboxMessages.Count > 0)
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
