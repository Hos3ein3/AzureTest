using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(255);
        builder.HasIndex(t => t.UserId);

        builder.HasMany(t => t.Messages)
               .WithOne(m => m.Ticket)
               .HasForeignKey(m => m.TicketId)
               .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}
