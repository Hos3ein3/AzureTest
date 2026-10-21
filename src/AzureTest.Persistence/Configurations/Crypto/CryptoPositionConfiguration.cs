using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoPositionConfiguration : IEntityTypeConfiguration<CryptoPosition>
{
    public void Configure(EntityTypeBuilder<CryptoPosition> b)
    {
        b.ToTable("CryptoPositions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoPosition.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Side)
            .HasConversion<int>();

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.Property(x => x.Quantity)
            .HasPrecision(CryptoPosition.Rules.QuantityPrecision, CryptoPosition.Rules.QuantityScale);

        b.Property(x => x.EntryPrice)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.CurrentPrice)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.ClosePrice)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.InitialMargin)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.LiquidationPrice)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.UnrealizedPnl)
            .HasPrecision(CryptoPosition.Rules.PnlPrecision, CryptoPosition.Rules.PnlScale);

        b.Property(x => x.RealizedPnl)
            .HasPrecision(CryptoPosition.Rules.PnlPrecision, CryptoPosition.Rules.PnlScale);

        b.Property(x => x.StopLoss)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.TakeProfit)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.TrailingStopDistance)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.HighestPriceReached)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.Property(x => x.LowestPriceReached)
            .HasPrecision(CryptoPosition.Rules.PricePrecision, CryptoPosition.Rules.PriceScale);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Status);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
