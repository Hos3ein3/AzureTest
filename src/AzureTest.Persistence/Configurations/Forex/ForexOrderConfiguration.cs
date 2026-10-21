using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexOrderConfiguration : IEntityTypeConfiguration<ForexOrder>
{
    public void Configure(EntityTypeBuilder<ForexOrder> b)
    {
        b.ToTable("ForexOrders");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(ForexOrder.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Direction)
            .HasConversion<int>();

        b.Property(x => x.OrderType)
            .HasConversion<int>();

        b.Property(x => x.Status)
            .HasConversion<int>();

        b.Property(x => x.LotSize)
            .HasPrecision(ForexOrder.Rules.LotSizePrecision, ForexOrder.Rules.LotSizeScale);

        b.Property(x => x.RequestedPrice)
            .HasPrecision(ForexOrder.Rules.PricePrecision, ForexOrder.Rules.PriceScale);

        b.Property(x => x.FillPrice)
            .HasPrecision(ForexOrder.Rules.PricePrecision, ForexOrder.Rules.PriceScale);

        b.Property(x => x.StopLoss)
            .HasPrecision(ForexOrder.Rules.PricePrecision, ForexOrder.Rules.PriceScale);

        b.Property(x => x.TakeProfit)
            .HasPrecision(ForexOrder.Rules.PricePrecision, ForexOrder.Rules.PriceScale);

        b.Property(x => x.SlippagePips)
            .HasPrecision(ForexOrder.Rules.PricePrecision, ForexOrder.Rules.PriceScale);

        b.Property(x => x.IdempotencyKey)
            .HasMaxLength(ForexOrder.Rules.IdempotencyKeyMaxLength)
            .IsRequired();

        b.Property(x => x.RejectionReason)
            .HasMaxLength(ForexOrder.Rules.NoteMaxLength);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.IdempotencyKey);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
