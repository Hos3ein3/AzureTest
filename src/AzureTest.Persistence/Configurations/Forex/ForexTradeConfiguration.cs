using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexTradeConfiguration : IEntityTypeConfiguration<ForexTrade>
{
    public void Configure(EntityTypeBuilder<ForexTrade> b)
    {
        b.ToTable("ForexTrades");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(ForexTrade.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Direction)
            .HasConversion<int>();

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.Property(x => x.ExitReason)
            .HasConversion<int>();

        b.Property(x => x.LotSize)
            .HasPrecision(ForexTrade.Rules.LotSizePrecision, ForexTrade.Rules.LotSizeScale);

        b.Property(x => x.OpenPrice)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.ClosePrice)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.StopLoss)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.TakeProfit)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.TrailingStopPips)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.MarginRequired)
            .HasPrecision(ForexTrade.Rules.ProfitPrecision, ForexTrade.Rules.ProfitScale);

        b.Property(x => x.SwapCost)
            .HasPrecision(ForexTrade.Rules.ProfitPrecision, ForexTrade.Rules.ProfitScale);

        b.Property(x => x.Commission)
            .HasPrecision(ForexTrade.Rules.ProfitPrecision, ForexTrade.Rules.ProfitScale);

        b.Property(x => x.GrossProfit)
            .HasPrecision(ForexTrade.Rules.ProfitPrecision, ForexTrade.Rules.ProfitScale);

        b.Property(x => x.NetProfit)
            .HasPrecision(ForexTrade.Rules.ProfitPrecision, ForexTrade.Rules.ProfitScale);

        b.Property(x => x.Pips)
            .HasPrecision(ForexTrade.Rules.PricePrecision, ForexTrade.Rules.PriceScale);

        b.Property(x => x.Notes)
            .HasMaxLength(ForexTrade.Rules.NotesMaxLength);

        b.Property(x => x.ScreenshotUrl)
            .HasMaxLength(ForexTrade.Rules.UrlMaxLength);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Symbol);
        b.HasIndex(x => x.Status);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
