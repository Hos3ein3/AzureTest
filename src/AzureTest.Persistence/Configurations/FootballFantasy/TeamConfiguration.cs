using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Team.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.ShortName)
            .HasMaxLength(Team.Rules.ShortNameMaxLength)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(Team.Rules.CodeMaxLength);

        builder.Property(x => x.StadiumName)
            .HasMaxLength(Team.Rules.StadiumNameMaxLength);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(Team.Rules.LogoUrlMaxLength);

        builder.Property(x => x.TeamType).HasConversion<int>();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
