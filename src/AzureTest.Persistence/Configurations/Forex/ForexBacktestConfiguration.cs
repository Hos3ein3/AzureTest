using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexBacktestConfiguration : IEntityTypeConfiguration<ForexBacktest>
{
    public void Configure(EntityTypeBuilder<ForexBacktest> b)
    {
        b.ToTable("ForexBacktests");
        b.HasKey(x => x.Id);

        b.Property(x => x.StrategyName)
            .HasMaxLength(ForexBacktest.Rules.StrategyNameMaxLength)
            .IsRequired();

        b.Property(x => x.Symbol)
            .HasMaxLength(ForexBacktest.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.Timeframe)
            .HasConversion<int>();

        b.Property(x => x.InitialBalance)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.FinalBalance)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.NetProfit)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.ReturnPercent)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.MaxDrawdownPercent)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.WinRatePercent)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.ProfitFactor)
            .HasPrecision(ForexBacktest.Rules.RatioPrecision, ForexBacktest.Rules.RatioScale);

        b.Property(x => x.ExpectancyPips)
            .HasPrecision(ForexBacktest.Rules.RatioPrecision, ForexBacktest.Rules.RatioScale);

        b.Property(x => x.SharpeRatio)
            .HasPrecision(ForexBacktest.Rules.RatioPrecision, ForexBacktest.Rules.RatioScale);

        b.Property(x => x.SpreadPips)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.SlippagePips)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.TotalSwapCost)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.OutOfSampleEfficiency)
            .HasPrecision(ForexBacktest.Rules.DecimalPrecision, ForexBacktest.Rules.DecimalScale);

        b.Property(x => x.TradeLogsJson)
            .HasMaxLength(ForexBacktest.Rules.JsonReportMaxLength);

        b.Property(x => x.Notes)
            .HasMaxLength(ForexBacktest.Rules.NotesMaxLength);

        b.HasIndex(x => x.StrategyId);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
