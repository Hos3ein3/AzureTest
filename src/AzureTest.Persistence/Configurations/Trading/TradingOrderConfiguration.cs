using AzureTest.Domain.Entities.Trading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Trading;

public sealed class TradingOrderConfiguration : IEntityTypeConfiguration<TradingOrder>
{
    public void Configure(EntityTypeBuilder<TradingOrder> b)
    {
        b.ToTable("TradingOrders");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(TradingOrder.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.AssetName)
            .HasMaxLength(TradingOrder.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.AssetClass)
            .HasConversion<int>();

        b.Property(x => x.Side)
            .HasConversion<int>();

        b.Property(x => x.OrderType)
            .HasConversion<int>();

        b.Property(x => x.Quantity)
            .HasPrecision(TradingOrder.Rules.QuantityPrecision, TradingOrder.Rules.QuantityScale);

        b.Property(x => x.Price)
            .HasPrecision(TradingOrder.Rules.PricePrecision, TradingOrder.Rules.PriceScale);

        b.Property(x => x.TriggerPrice)
            .HasPrecision(TradingOrder.Rules.PricePrecision, TradingOrder.Rules.PriceScale);

        b.Property(x => x.StopLoss)
            .HasPrecision(TradingOrder.Rules.PricePrecision, TradingOrder.Rules.PriceScale);

        b.Property(x => x.TakeProfit)
            .HasPrecision(TradingOrder.Rules.PricePrecision, TradingOrder.Rules.PriceScale);

        b.Property(x => x.MultipleTakeProfitsJson)
            .HasMaxLength(TradingOrder.Rules.JsonMaxLength);

        b.Property(x => x.TrailingStopDistance)
            .HasPrecision(TradingOrder.Rules.PricePrecision, TradingOrder.Rules.PriceScale);

        b.Property(x => x.TimeInForce)
            .HasConversion<int>();

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
