namespace EnterpriseKit.Infrastructure.Persistence.Configurations;

using EnterpriseKit.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnType("jsonb")  // Postgres JSONB for efficient JSON querying
            .IsRequired();

        builder.Property(m => m.Error)
            .HasMaxLength(2000);

        // Partial index for fast polling: only unprocessed messages
        builder.HasIndex(m => new { m.ProcessedAt, m.CreatedAt })
            .HasDatabaseName("ix_outbox_unprocessed")
            .HasFilter("processed_at IS NULL");
    }
}
