namespace EnterpriseKit.Infrastructure.Outbox;

using System.Text.Json;
using EnterpriseKit.Domain.Common;
using EnterpriseKit.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that polls the <c>outbox_messages</c> table and
/// dispatches unprocessed domain events via MediatR.
///
/// Guarantees at-least-once delivery. Handlers must be idempotent.
/// </summary>
public sealed class OutboxRelayService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxRelayService> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxRetries = 5;

    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox relay service started. Polling every {Interval}s", PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in OutboxRelayService");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        logger.LogInformation("Outbox relay service stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        foreach (var msg in messages)
        {
            try
            {
                var eventType = Type.GetType(msg.Type);
                if (eventType is null)
                {
                    logger.LogError("Cannot resolve event type: {TypeName}", msg.Type);
                    msg.MarkFailed($"Cannot resolve type: {msg.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(msg.Payload, eventType, JsonOpts);
                if (domainEvent is IDomainEvent evt)
                {
                    await publisher.Publish(evt, ct);
                    msg.MarkProcessed();
                    logger.LogDebug("Published event {EventType} ({EventId})", eventType.Name, evt.EventId);
                }
                else
                {
                    msg.MarkFailed("Deserialized object is not an IDomainEvent.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {Id} (attempt {Attempt})",
                    msg.Id, msg.RetryCount + 1);
                msg.MarkFailed(ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
