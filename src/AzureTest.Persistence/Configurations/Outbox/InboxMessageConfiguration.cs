using AzureTest.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Outbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> b)
    {
        b.ToTable("InboxMessages");
        b.HasKey(x => new { x.EventId, x.HandlerName });

        b.Property(x => x.HandlerName)
            .HasMaxLength(200)
            .IsRequired();
        
    }
}
