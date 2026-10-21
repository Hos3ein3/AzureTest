using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoJournalEntryConfiguration : IEntityTypeConfiguration<CryptoJournalEntry>
{
    public void Configure(EntityTypeBuilder<CryptoJournalEntry> b)
    {
        b.ToTable("CryptoJournalEntries");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoJournalEntry.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.StrategyTag)
            .HasMaxLength(CryptoJournalEntry.Rules.TagMaxLength);

        b.Property(x => x.EmotionTag)
            .HasMaxLength(CryptoJournalEntry.Rules.EmotionMaxLength);

        b.Property(x => x.Notes)
            .HasMaxLength(CryptoJournalEntry.Rules.NotesMaxLength);

        b.Property(x => x.LessonsLearned)
            .HasMaxLength(CryptoJournalEntry.Rules.LessonsMaxLength);

        b.HasIndex(x => x.PositionId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
