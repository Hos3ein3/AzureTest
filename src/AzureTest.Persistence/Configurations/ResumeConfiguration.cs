

using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PositionTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Location)
            .HasMaxLength(200);
            
        builder.Property(r => r.JobLink)
            .HasMaxLength(500);

        builder.Property(r => r.Source)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(r => r.Description)
            .HasConversion<string>()
            .HasMaxLength(10000);
        
        builder.Property(r => r.Note)
            .HasConversion<string>()
            .HasMaxLength(5000);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        // Ignore domain events from EF Core's perspective
        builder.Ignore(r => r.DomainEvents);
    }
}
