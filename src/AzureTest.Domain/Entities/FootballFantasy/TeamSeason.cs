using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class TeamSeason : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int GroupNameMaxLength = 50;
    }

    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public string? GroupName { get; private set; }

    public ICollection<PlayerSquad> PlayerSquads { get; private set; } = new List<PlayerSquad>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private TeamSeason() { }

    public static TeamSeason Create(Guid teamId, Guid seasonId, string? groupName = null)
    {
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));

        return new TeamSeason
        {
            TeamId = teamId,
            SeasonId = seasonId,
            GroupName = groupName?.Trim()
        };
    }
}
