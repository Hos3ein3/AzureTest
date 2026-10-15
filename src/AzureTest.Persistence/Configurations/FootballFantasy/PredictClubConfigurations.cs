using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class MatchStatConfiguration : IEntityTypeConfiguration<MatchStat>
{
    public void Configure(EntityTypeBuilder<MatchStat> builder)
    {
        builder.ToTable("MatchStats", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Fixture)
            .WithOne()
            .HasForeignKey<MatchStat>(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MvpPlayer)
            .WithMany()
            .HasForeignKey(x => x.MvpPlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class PredictClubConfiguration : IEntityTypeConfiguration<PredictClub>
{
    public void Configure(EntityTypeBuilder<PredictClub> builder)
    {
        builder.ToTable("PredictClubs", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(PredictClub.Rules.NameMaxLength).IsRequired();
        builder.Property(x => x.ClubTag).HasMaxLength(PredictClub.Rules.ClubTagMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(PredictClub.Rules.DescriptionMaxLength);
        builder.Property(x => x.LogoUrl).HasColumnType("text");
        builder.Property(x => x.JoinCode).HasMaxLength(PredictClub.Rules.JoinCodeMaxLength);

        builder.Property(x => x.Visibility).HasConversion<int>();
        builder.Property(x => x.Format).HasConversion<int>();
        builder.Property(x => x.ScopeType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => x.ClubTag).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class PredictClubMemberConfiguration : IEntityTypeConfiguration<PredictClubMember>
{
    public void Configure(EntityTypeBuilder<PredictClubMember> builder)
    {
        builder.ToTable("PredictClubMembers", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.PredictClub)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.PredictClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PredictClubId, x.UserId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class MatchPredictionConfiguration : IEntityTypeConfiguration<MatchPrediction>
{
    public void Configure(EntityTypeBuilder<MatchPrediction> builder)
    {
        builder.ToTable("MatchPredictions", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.PredictClub)
            .WithMany(x => x.Predictions)
            .HasForeignKey(x => x.PredictClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Fixture)
            .WithMany()
            .HasForeignKey(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PredictedMvpPlayer)
            .WithMany()
            .HasForeignKey(x => x.PredictedMvpPlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.PredictClubId, x.FixtureId, x.UserId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class TournamentPredictionConfiguration : IEntityTypeConfiguration<TournamentPrediction>
{
    public void Configure(EntityTypeBuilder<TournamentPrediction> builder)
    {
        builder.ToTable("TournamentPredictions", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.PredictClub)
            .WithMany(x => x.TournamentPredictions)
            .HasForeignKey(x => x.PredictClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FirstPlaceTeam).WithMany().HasForeignKey(x => x.FirstPlaceTeamId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.SecondPlaceTeam).WithMany().HasForeignKey(x => x.SecondPlaceTeamId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ThirdPlaceTeam).WithMany().HasForeignKey(x => x.ThirdPlaceTeamId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.BestPlayer).WithMany().HasForeignKey(x => x.BestPlayerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.TopScorerPlayer).WithMany().HasForeignKey(x => x.TopScorerPlayerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.TopAssisterPlayer).WithMany().HasForeignKey(x => x.TopAssisterPlayerId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.PredictClubId, x.UserId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
