using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexJournalEntryConfiguration : IEntityTypeConfiguration<ForexJournalEntry>
{
    public void Configure(EntityTypeBuilder<ForexJournalEntry> b)
    {
        b.ToTable("ForexJournalEntries");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(ForexJournalEntry.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.MarketSession)
            .HasMaxLength(ForexJournalEntry.Rules.SessionMaxLength)
            .IsRequired();

        b.Property(x => x.SetupType)
            .HasMaxLength(ForexJournalEntry.Rules.SetupTypeMaxLength)
            .IsRequired();

        b.Property(x => x.Emotions)
            .HasMaxLength(ForexJournalEntry.Rules.EmotionsMaxLength);

        b.Property(x => x.Mistakes)
            .HasMaxLength(ForexJournalEntry.Rules.MistakesMaxLength);

        b.Property(x => x.LessonsLearned)
            .HasMaxLength(ForexJournalEntry.Rules.LessonsMaxLength);

        b.Property(x => x.DetailedNotes)
            .HasMaxLength(ForexJournalEntry.Rules.NotesMaxLength);

        b.Property(x => x.ScreenshotUrl)
            .HasMaxLength(ForexJournalEntry.Rules.UrlMaxLength);

        b.HasIndex(x => x.TradeId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Symbol);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
