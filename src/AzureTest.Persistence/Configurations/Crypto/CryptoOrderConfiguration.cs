using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoOrderConfiguration : IEntityTypeConfiguration<CryptoOrder>
{
    public void Configure(EntityTypeBuilder<CryptoOrder> b)
    {
        b.ToTable("CryptoOrders");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoOrder.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Side)
            .HasConversion<int>();

        b.Property(x => x.OrderType)
            .HasConversion<int>();

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.Property(x => x.TradingMode)
            .HasConversion<int>();

        b.Property(x => x.Quantity)
            .HasPrecision(CryptoOrder.Rules.QuantityPrecision, CryptoOrder.Rules.QuantityScale);

        b.Property(x => x.FilledQuantity)
            .HasPrecision(CryptoOrder.Rules.QuantityPrecision, CryptoOrder.Rules.QuantityScale);

        b.Property(x => x.RequestedPrice)
            .HasPrecision(CryptoOrder.Rules.PricePrecision, CryptoOrder.Rules.PriceScale);

        b.Property(x => x.StopPrice)
            .HasPrecision(CryptoOrder.Rules.PricePrecision, CryptoOrder.Rules.PriceScale);

        b.Property(x => x.FillPrice)
            .HasPrecision(CryptoOrder.Rules.PricePrecision, CryptoOrder.Rules.PriceScale);

        b.Property(x => x.Commission)
            .HasPrecision(CryptoOrder.Rules.CommissionPrecision, CryptoOrder.Rules.CommissionScale);

        b.Property(x => x.Slippage)
            .HasPrecision(CryptoOrder.Rules.PricePrecision, CryptoOrder.Rules.PriceScale);

        b.Property(x => x.IdempotencyKey)
            .HasMaxLength(CryptoOrder.Rules.IdempotencyKeyMaxLength)
            .IsRequired();

        b.Property(x => x.ClientOrderId)
            .HasMaxLength(CryptoOrder.Rules.ClientOrderIdMaxLength)
            .IsRequired();

        b.Property(x => x.RejectionReason)
            .HasMaxLength(CryptoOrder.Rules.RejectionReasonMaxLength);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.IdempotencyKey);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
