using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyTeam : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int LogoUrlMaxLength = 500;
        public const int CreditsPrecision = 10;
        public const int CreditsScale = 2;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public Guid? CoachId { get; private set; }
    public Coach? Coach { get; private set; }
    public decimal CreditsLeft { get; private set; }
    public int TotalPoints { get; private set; }
    public int GameweekTransfersMade { get; private set; }
    public bool IsFinalized { get; private set; }
    public bool IsApproved { get; private set; } = true;
    public bool IsSpectator { get; private set; }

    public ICollection<FantasySquadEntry> SquadEntries { get; private set; } = new List<FantasySquadEntry>();
    public ICollection<FantasyLineup> Lineups { get; private set; } = new List<FantasyLineup>();
    public ICollection<FantasyTeamPeriodScore> PeriodScores { get; private set; } = new List<FantasyTeamPeriodScore>();
    public ICollection<FantasyTeamChip> Chips { get; private set; } = new List<FantasyTeamChip>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyTeam() { }

    public static FantasyTeam Create(
        Guid fantasyLeagueId,
        Guid userId,
        string name,
        decimal initialBudget,
        string? logoUrl = null,
        Guid? coachId = null)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid fantasy team name.", nameof(name));
        if (initialBudget < 0) throw new ArgumentException("Initial budget cannot be negative.", nameof(initialBudget));

        return new FantasyTeam
        {
            FantasyLeagueId = fantasyLeagueId,
            UserId = userId,
            Name = name.Trim(),
            LogoUrl = logoUrl?.Trim(),
            CoachId = coachId,
            CreditsLeft = initialBudget,
            TotalPoints = 0
        };
    }

    public void SetCoach(Guid? coachId)
    {
        CoachId = coachId;
    }

    public void DeductCredits(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (CreditsLeft - amount < 0) throw new InvalidOperationException("Insufficient credits.");
        CreditsLeft -= amount;
    }

    public void AddCredits(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        CreditsLeft += amount;
    }

    public void FinalizeSquad() => IsFinalized = true;
    public void UnfinalizeSquad() => IsFinalized = false;
    public void ApproveTeam() => IsApproved = true;
    public void RejectTeam() => IsApproved = false;
    public void MakeSpectator() => IsSpectator = true;

    public void AddPoints(int points) => TotalPoints += points;
    public void DeductPoints(int points) => TotalPoints -= points;

    public void RecordTransfer(int pointPenalty = 0)
    {
        GameweekTransfersMade++;
        if (pointPenalty > 0)
        {
            DeductPoints(pointPenalty);
        }
    }

    public void ResetGameweekTransfers()
    {
        GameweekTransfersMade = 0;
    }
}
