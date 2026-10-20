using AzureTest.Domain.Entities.Pomodoro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public sealed class PomodoroSettingConfiguration : IEntityTypeConfiguration<PomodoroSetting>
{
    public void Configure(EntityTypeBuilder<PomodoroSetting> builder)
    {
        builder.ToTable("PomodoroSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.WorkDurationMinutes)
            .IsRequired()
            .HasDefaultValue(PomodoroSetting.Rules.DefaultWorkDurationMinutes);

        builder.Property(x => x.ShortBreakMinutes)
            .IsRequired()
            .HasDefaultValue(PomodoroSetting.Rules.DefaultShortBreakMinutes);

        builder.Property(x => x.LongBreakMinutes)
            .IsRequired()
            .HasDefaultValue(PomodoroSetting.Rules.DefaultLongBreakMinutes);

        builder.Property(x => x.LongBreakInterval)
            .IsRequired()
            .HasDefaultValue(PomodoroSetting.Rules.DefaultLongBreakInterval);

        builder.Property(x => x.DefaultSkin)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.TenantId);
    }
}
