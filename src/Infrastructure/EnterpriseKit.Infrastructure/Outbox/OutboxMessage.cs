namespace EnterpriseKit.Infrastructure.Outbox;

/// <summary>
/// Represents a serialized domain event waiting to be published.
/// Rows are written inside the same DB transaction as the aggregate save,
/// guaranteeing atomicity via the Transactional Outbox Pattern.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Unique message ID.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Assembly-qualified type name of the domain event (for deserialization).</summary>
    public string Type { get; init; } = null!;

    /// <summary>JSON-serialized domain event payload.</summary>
    public string Payload { get; init; } = null!;

    /// <summary>UTC time this message was created.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>UTC time this message was successfully processed. Null = not yet processed.</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>Error message if processing failed. Null = success or not yet attempted.</summary>
    public string? Error { get; private set; }

    /// <summary>Number of processing attempts so far.</summary>
    public int RetryCount { get; private set; }

    /// <summary>Marks the message as successfully processed.</summary>
    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    /// <summary>Records a failed processing attempt.</summary>
    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
