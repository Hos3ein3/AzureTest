using AzureTest.Domain.Entities.Trading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Trading;

public sealed class TradingPositionConfiguration : IEntityTypeConfiguration<TradingPosition>
{
    public void Configure(EntityTypeBuilder<TradingPosition> b)
    {
        b.ToTable("TradingPositions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(TradingPosition.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.AssetName)
            .HasMaxLength(TradingPosition.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.AssetClass)
            .HasConversion<int>();

        b.Property(x => x.Side)
            .HasConversion<int>();

        b.Property(x => x.Quantity)
            .HasPrecision(TradingPosition.Rules.QuantityPrecision, TradingPosition.Rules.QuantityScale);

        b.Property(x => x.EntryPrice)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.CurrentPrice)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.ClosePrice)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.InitialMargin)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.UnrealizedPnl)
            .HasPrecision(TradingPosition.Rules.PnlPrecision, TradingPosition.Rules.PnlScale);

        b.Property(x => x.RealizedPnl)
            .HasPrecision(TradingPosition.Rules.PnlPrecision, TradingPosition.Rules.PnlScale);

        b.Property(x => x.StopLoss)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.TakeProfit)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.TrailingStopDistance)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.HighestPriceReached)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.LowestPriceReached)
            .HasPrecision(TradingPosition.Rules.PricePrecision, TradingPosition.Rules.PriceScale);

        b.Property(x => x.MultipleTakeProfitsJson)
            .HasMaxLength(TradingPosition.Rules.JsonMaxLength);

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Symbol);
        b.HasIndex(x => x.Status);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
