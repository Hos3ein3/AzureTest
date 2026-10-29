using AzureTest.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(x => x.Id);

        b.Property(x => x.EventType)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Payload)
            .IsRequired();

        b.Property(x => x.Error)
            .HasMaxLength(2000);

        b.HasIndex(x => new { x.ProcessedOnUtc, x.RetryCount });
    }
}
