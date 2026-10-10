using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Season : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
    }

    public Guid LeagueId { get; private set; }
    public League League { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsCurrent { get; private set; }

    public ICollection<Stage> Stages { get; private set; } = new List<Stage>();
    public ICollection<TeamSeason> TeamSeasons { get; private set; } = new List<TeamSeason>();
    public ICollection<Fixture> Fixtures { get; private set; } = new List<Fixture>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Season() { }

    public static Season Create(Guid leagueId, string name, DateTime startDate, DateTime endDate, bool isCurrent = false)
    {
        if (leagueId == Guid.Empty) throw new ArgumentException("LeagueId is required.", nameof(leagueId));
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid season name.", nameof(name));
        if (endDate < startDate)
            throw new ArgumentException("EndDate must be on or after StartDate.", nameof(endDate));

        return new Season
        {
            LeagueId = leagueId,
            Name = name.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsCurrent = isCurrent
        };
    }

    public void SetCurrent(bool isCurrent) => IsCurrent = isCurrent;

    public void Update(string name, DateTime startDate, DateTime endDate, bool isCurrent)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid season name.", nameof(name));
        if (endDate < startDate)
            throw new ArgumentException("EndDate must be on or after StartDate.", nameof(endDate));

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
    }
}
