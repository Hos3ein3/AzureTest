using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoBacktestConfiguration : IEntityTypeConfiguration<CryptoBacktest>
{
    public void Configure(EntityTypeBuilder<CryptoBacktest> b)
    {
        b.ToTable("CryptoBacktests");
        b.HasKey(x => x.Id);

        b.Property(x => x.StrategyName)
            .HasMaxLength(CryptoBacktest.Rules.StrategyNameMaxLength)
            .IsRequired();

        b.Property(x => x.StrategyType)
            .HasConversion<int>();

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoBacktest.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Timeframe)
            .HasConversion<int>();

        b.Property(x => x.InitialCapital)
            .HasPrecision(CryptoBacktest.Rules.CapitalPrecision, CryptoBacktest.Rules.CapitalScale);

        b.Property(x => x.FinalCapital)
            .HasPrecision(CryptoBacktest.Rules.CapitalPrecision, CryptoBacktest.Rules.CapitalScale);

        b.Property(x => x.WinRate)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.ProfitFactor)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.MaxDrawdownPercent)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.SharpeRatio)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.CommissionFeePercent)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.SlippagePercent)
            .HasPrecision(CryptoBacktest.Rules.MetricPrecision, CryptoBacktest.Rules.MetricScale);

        b.Property(x => x.TotalFeesPaid)
            .HasPrecision(CryptoBacktest.Rules.CapitalPrecision, CryptoBacktest.Rules.CapitalScale);

        b.Property(x => x.EquityCurveJson)
            .HasMaxLength(CryptoBacktest.Rules.JsonDataMaxLength);

        b.Property(x => x.WalkForwardSummaryJson)
            .HasMaxLength(CryptoBacktest.Rules.JsonDataMaxLength);

        b.HasIndex(x => x.StrategyId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
