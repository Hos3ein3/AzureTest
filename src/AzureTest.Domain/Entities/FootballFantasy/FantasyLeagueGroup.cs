using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeagueGroup : BaseEntity, IAudit
{
    public static class Rules
    {
        public const int NameMaxLength = 50;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public string Name { get; private set; } = default!; // e.g. "Group A"
    public int GroupNumber { get; private set; } // 1, 2, ...

    public ICollection<FantasyMatchup> Matchups { get; private set; } = new List<FantasyMatchup>();

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeagueGroup() { }

    public static FantasyLeagueGroup Create(Guid fantasyLeagueId, string name, int groupNumber)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid group name.", nameof(name));

        return new FantasyLeagueGroup
        {
            FantasyLeagueId = fantasyLeagueId,
            Name = name.Trim(),
            GroupNumber = groupNumber
        };
    }
}
