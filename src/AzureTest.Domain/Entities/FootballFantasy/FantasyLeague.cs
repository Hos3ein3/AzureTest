using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeague : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 3;
        public const int NameMaxLength = 100;
        public const int LeagueTagMinLength = 10;
        public const int LeagueTagMaxLength = 12;
        public const string LeagueTagPattern = "^[A-Z0-9]{10,12}$";
        public const int DescriptionMaxLength = 500;
        public const int LogoUrlMaxLength = 2000;
        public const int JoinCodeMaxLength = 20;
        public const int AllowedFormationsMaxLength = 500;
        public const string DefaultAllowedFormations = "4-3-3, 4-4-2, 3-5-2, 4-2-3-1, 3-4-3, 5-3-2, 5-4-1, 4-5-1, 4-1-4-1, 4-3-1-2, 4-1-2-1-2, 3-4-1-2, 3-4-2-1, 4-2-2-2, 5-2-3, 4-2-4, 4-3-2-1, 3-3-4";
        public const int DefaultSquadSize = 15;
        public const int DefaultStartingSize = 11;
        public const int DefaultBenchSize = 4;
        public const int MinLeagueMembers = 4;
        public const int MaxLeagueMembers = 48;
        public static readonly decimal DefaultBudget = 100.0m;
        public static readonly decimal DefaultCaptainMultiplier = 2.0m;
        public const int BudgetPrecision = 10;
        public const int BudgetScale = 2;
        public const int MultiplierPrecision = 4;
        public const int MultiplierScale = 2;
    }

    public Guid? OwnerUserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string LeagueTag { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public FantasyLeagueVisibility Visibility { get; private set; }
    public FantasyLeagueMode Mode { get; private set; }
    public FantasyLeagueFormat Format { get; private set; } = FantasyLeagueFormat.TotalPoints;
    public FantasyPeriodType PeriodType { get; private set; }
    public FantasySquadMode SquadMode { get; private set; }
    public int SquadSize { get; private set; }
    public int StartingSize { get; private set; }
    public int MinSquadSize { get; private set; }
    public int BenchSize { get; private set; }
    public decimal Budget { get; private set; }
    public int? MaxPlayersPerRealTeam { get; private set; }
    public string? AllowedFormations { get; private set; }
    public bool CaptainEnabled { get; private set; }
    public decimal CaptainMultiplier { get; private set; }
    public bool AllowCoach { get; private set; }
    public bool AllowAutoSub { get; private set; }
    public bool EnableMvpBonus { get; private set; }
    public int MvpBonusPoints { get; private set; }
    public int? GroupCount { get; private set; }
    public int? MaxMembers { get; private set; }
    public string? JoinCode { get; private set; }
    public FantasyKickoutType KickoutType { get; private set; } = FantasyKickoutType.FullRemoval;
    public DateTime? AuctionStartsAt { get; private set; }
    public DateTime? AuctionEndsAt { get; private set; }
    public Guid ScoringRuleSetId { get; private set; }
    public ScoringRuleSet ScoringRuleSet { get; private set; } = default!;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public DateTime? CloseTime { get; private set; }
    public FantasyLeagueStatus Status { get; private set; }

    // Real-world Transfer Market Windows (e.g. Summer & Winter, or single tournament window)
    public DateTime? Window1Open { get; private set; }
    public DateTime? Window1Close { get; private set; }
    public DateTime? Window2Open { get; private set; }
    public DateTime? Window2Close { get; private set; }

    public bool IsTransferMarketOpen(DateTime? checkDate = null)
    {
        var now = checkDate ?? DateTime.UtcNow;
        
        // If no windows configured, default to league lifetime open before closing
        if (!Window1Open.HasValue && !Window1Close.HasValue && !Window2Open.HasValue && !Window2Close.HasValue)
        {
            return now >= StartsAt && (CloseTime == null || now <= CloseTime.Value);
        }

        bool inWindow1 = Window1Open.HasValue && Window1Close.HasValue && now >= Window1Open.Value && now <= Window1Close.Value;
        bool inWindow2 = Window2Open.HasValue && Window2Close.HasValue && now >= Window2Open.Value && now <= Window2Close.Value;

        return inWindow1 || inWindow2;
    }

    public ICollection<FantasyLeaguePositionLimit> PositionLimits { get; private set; } = new List<FantasyLeaguePositionLimit>();
    public ICollection<FantasyLeagueSource> Sources { get; private set; } = new List<FantasyLeagueSource>();
    public ICollection<FantasyLeaguePoolRule> PoolRules { get; private set; } = new List<FantasyLeaguePoolRule>();
    public ICollection<FantasyLeagueMember> Members { get; private set; } = new List<FantasyLeagueMember>();
    public ICollection<FantasyLeagueInvite> Invites { get; private set; } = new List<FantasyLeagueInvite>();
    public ICollection<FantasyPeriod> Periods { get; private set; } = new List<FantasyPeriod>();
    public ICollection<FantasyTeam> FantasyTeams { get; private set; } = new List<FantasyTeam>();
    public ICollection<FantasyLeagueGroup> Groups { get; private set; } = new List<FantasyLeagueGroup>();
    public ICollection<FantasyMatchup> Matchups { get; private set; } = new List<FantasyMatchup>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeague() { }

    public static FantasyLeague Create(
        string name,
        string leagueTag,
        FantasyLeagueVisibility visibility,
        FantasyLeagueMode mode,
        FantasyPeriodType periodType,
        FantasySquadMode squadMode,
        int squadSize,
        int startingSize,
        decimal budget,
        Guid scoringRuleSetId,
        DateTime startsAt,
        DateTime endsAt,
        Guid? ownerUserId = null,
        string? description = null,
        int? maxPlayersPerRealTeam = null,
        string? allowedFormations = null,
        bool captainEnabled = true,
        decimal? captainMultiplier = null,
        bool allowCoach = true,
        int? maxMembers = null,
        string? joinCode = null,
        DateTime? closeTime = null,
        string? logoUrl = null,
        DateTime? window1Open = null,
        DateTime? window1Close = null,
        DateTime? window2Open = null,
        DateTime? window2Close = null,
        int? minSquadSize = null,
        FantasyKickoutType kickoutType = FantasyKickoutType.FullRemoval,
        DateTime? auctionStartsAt = null,
        DateTime? auctionEndsAt = null,
        FantasyLeagueFormat format = FantasyLeagueFormat.TotalPoints,
        bool allowAutoSub = false,
        bool enableMvpBonus = false,
        int mvpBonusPoints = 3,
        int? groupCount = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid league name.", nameof(name));
        if (string.IsNullOrWhiteSpace(leagueTag) || leagueTag.Length < Rules.LeagueTagMinLength || leagueTag.Length > Rules.LeagueTagMaxLength)
            throw new ArgumentException($"LeagueTag must be between {Rules.LeagueTagMinLength} and {Rules.LeagueTagMaxLength} characters.", nameof(leagueTag));
        if (startingSize > squadSize)
            throw new ArgumentException("StartingSize cannot exceed SquadSize.");
        
        var effectiveMinSquad = minSquadSize ?? startingSize;
        if (effectiveMinSquad < startingSize || effectiveMinSquad > squadSize)
            throw new ArgumentException($"MinSquadSize must be between {startingSize} and {squadSize}.", nameof(minSquadSize));

        if (closeTime.HasValue && closeTime.Value >= endsAt)
            throw new ArgumentException("CloseTime must be before EndsAt.");
        if (visibility == FantasyLeagueVisibility.Global && ownerUserId.HasValue)
            throw new ArgumentException("Global leagues must not have an owner.");
        if (maxMembers.HasValue && (maxMembers.Value < Rules.MinLeagueMembers || maxMembers.Value > Rules.MaxLeagueMembers))
            throw new ArgumentException($"Maximum number of teams/players must be between {Rules.MinLeagueMembers} and {Rules.MaxLeagueMembers}.");

        var cleanTag = leagueTag.Trim().ToUpperInvariant();

        return new FantasyLeague
        {
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            LeagueTag = cleanTag,
            Description = description?.Trim(),
            LogoUrl = logoUrl?.Trim(),
            Visibility = visibility,
            Mode = mode,
            Format = format,
            PeriodType = periodType,
            SquadMode = squadMode,
            SquadSize = squadSize,
            StartingSize = startingSize,
            MinSquadSize = effectiveMinSquad,
            BenchSize = squadSize - startingSize,
            Budget = budget,
            MaxPlayersPerRealTeam = maxPlayersPerRealTeam,
            AllowedFormations = allowedFormations?.Trim(),
            CaptainEnabled = captainEnabled,
            CaptainMultiplier = captainMultiplier ?? Rules.DefaultCaptainMultiplier,
            AllowCoach = allowCoach,
            AllowAutoSub = allowAutoSub,
            EnableMvpBonus = enableMvpBonus,
            MvpBonusPoints = mvpBonusPoints,
            GroupCount = groupCount,
            MaxMembers = maxMembers ?? Rules.MaxLeagueMembers,
            JoinCode = joinCode?.Trim(),
            KickoutType = kickoutType,
            AuctionStartsAt = auctionStartsAt,
            AuctionEndsAt = auctionEndsAt,
            ScoringRuleSetId = scoringRuleSetId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            CloseTime = closeTime,
            Status = FantasyLeagueStatus.Draft,
            Window1Open = window1Open,
            Window1Close = window1Close,
            Window2Open = window2Open,
            Window2Close = window2Close
        };
    }

    public void OpenLeague()
    {
        if (Status != FantasyLeagueStatus.Draft)
            throw new InvalidOperationException("Only draft leagues can be opened.");
        if (Sources.Count == 0 || Periods.Count == 0)
            throw new InvalidOperationException("League must have at least one source season and one period to open.");
        Status = FantasyLeagueStatus.Open;
    }

    public void ActivateLeague()
    {
        if (Status != FantasyLeagueStatus.Open)
            throw new InvalidOperationException("Only open leagues can become active.");
        Status = FantasyLeagueStatus.Active;
    }

    public bool CanModifyRules(DateTime? checkDate = null)
    {
        var now = checkDate ?? DateTime.UtcNow;
        // League rules cannot be changed if league has already started or status is active/finished
        return Status == FantasyLeagueStatus.Draft || (Status == FantasyLeagueStatus.Open && now < StartsAt);
    }

    public void AssertCanModifyRules(DateTime? checkDate = null)
    {
        if (!CanModifyRules(checkDate))
        {
            throw new InvalidOperationException("Fantasy league rules cannot be changed after the league has started.");
        }
    }
}
