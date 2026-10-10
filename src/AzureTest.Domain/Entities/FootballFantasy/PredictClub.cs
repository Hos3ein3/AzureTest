using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

/// <summary>
/// Predict Club (Fantasy Predict Club) where users predict match facts, scores, stats, and tournament outcomes.
/// Supports Global, Private, and Public visibility, custom scoring rules per prediction item, format modes, and rule-locking.
/// </summary>
public sealed class PredictClub : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 3;
        public const int NameMaxLength = 100;
        public const int ClubTagMinLength = 10;
        public const int ClubTagMaxLength = 12;
        public const string ClubTagPattern = "^[A-Z0-9]{10,12}$";
        public const int DescriptionMaxLength = 500;
        public const int LogoUrlMaxLength = 2000;
        public const int JoinCodeMaxLength = 20;
        public const int MinClubMembers = 2;
        public const int MaxClubMembers = 1000;
        public const int DefaultExactScorePoints = 10;
        public const int DefaultOutcomePoints = 5;
        public const int DefaultFactPoints = 3;
        public const int DefaultTournamentOutcomePoints = 25;
    }

    public Guid? OwnerUserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string ClubTag { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public FantasyLeagueVisibility Visibility { get; private set; }
    public FantasyLeagueFormat Format { get; private set; } = FantasyLeagueFormat.TotalPoints;
    public PoolScopeType ScopeType { get; private set; } = PoolScopeType.All;
    public string ScopeValue { get; private set; } = "All";
    public int? MaxMembers { get; private set; }
    public string? JoinCode { get; private set; }
    public PredictClubStatus Status { get; private set; } = PredictClubStatus.Draft;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    // Scoring configurations determined by creator
    public int ExactScorePoints { get; private set; } = Rules.DefaultExactScorePoints;
    public int OutcomePoints { get; private set; } = Rules.DefaultOutcomePoints;
    public int HalfTimeScorePoints { get; private set; } = Rules.DefaultFactPoints;
    public int BothTeamsToScorePoints { get; private set; } = Rules.DefaultFactPoints;
    public int OverUnderPoints { get; private set; } = Rules.DefaultFactPoints;
    public int YellowCardsPoints { get; private set; } = Rules.DefaultFactPoints;
    public int CornersPoints { get; private set; } = Rules.DefaultFactPoints;
    public int MatchMvpPoints { get; private set; } = Rules.DefaultFactPoints;
    public int FirstGoalMinutePoints { get; private set; } = Rules.DefaultFactPoints;

    // Tournament prediction points
    public bool EnableTournamentPredictions { get; private set; }
    public int TournamentPodiumPoints { get; private set; } = Rules.DefaultTournamentOutcomePoints;
    public int TournamentAwardPoints { get; private set; } = Rules.DefaultTournamentOutcomePoints;

    // Feature toggles for enabled match facts
    public bool PredictExactScore { get; private set; } = true;
    public bool PredictOutcome { get; private set; } = true;
    public bool PredictHalfTime { get; private set; } = true;
    public bool PredictBtts { get; private set; } = true;
    public bool PredictCards { get; private set; } = true;
    public bool PredictCorners { get; private set; } = true;
    public bool PredictMvp { get; private set; } = true;

    public ICollection<PredictClubMember> Members { get; private set; } = new List<PredictClubMember>();
    public ICollection<MatchPrediction> Predictions { get; private set; } = new List<MatchPrediction>();
    public ICollection<TournamentPrediction> TournamentPredictions { get; private set; } = new List<TournamentPrediction>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PredictClub() { }

    public static PredictClub Create(
        Guid? ownerUserId,
        string name,
        string clubTag,
        DateTime startsAt,
        DateTime endsAt,
        FantasyLeagueVisibility visibility = FantasyLeagueVisibility.Public,
        FantasyLeagueFormat format = FantasyLeagueFormat.TotalPoints,
        PoolScopeType scopeType = PoolScopeType.All,
        string scopeValue = "All",
        string? description = null,
        string? logoUrl = null,
        int? maxMembers = null,
        string? joinCode = null,
        int exactScorePoints = Rules.DefaultExactScorePoints,
        int outcomePoints = Rules.DefaultOutcomePoints,
        int halfTimeScorePoints = Rules.DefaultFactPoints,
        int bothTeamsToScorePoints = Rules.DefaultFactPoints,
        int overUnderPoints = Rules.DefaultFactPoints,
        int yellowCardsPoints = Rules.DefaultFactPoints,
        int cornersPoints = Rules.DefaultFactPoints,
        int matchMvpPoints = Rules.DefaultFactPoints,
        int firstGoalMinutePoints = Rules.DefaultFactPoints,
        bool enableTournamentPredictions = true,
        int tournamentPodiumPoints = Rules.DefaultTournamentOutcomePoints,
        int tournamentAwardPoints = Rules.DefaultTournamentOutcomePoints,
        bool predictExactScore = true,
        bool predictOutcome = true,
        bool predictHalfTime = true,
        bool predictBtts = true,
        bool predictCards = true,
        bool predictCorners = true,
        bool predictMvp = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Predict Club name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(clubTag)) throw new ArgumentException("ClubTag is required.", nameof(clubTag));
        if (endsAt <= startsAt) throw new ArgumentException("EndsAt must be after StartsAt.");

        return new PredictClub
        {
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            ClubTag = clubTag.Trim().ToUpperInvariant(),
            StartsAt = startsAt,
            EndsAt = endsAt,
            Visibility = visibility,
            Format = format,
            ScopeType = scopeType,
            ScopeValue = string.IsNullOrWhiteSpace(scopeValue) ? "All" : scopeValue.Trim(),
            Description = description?.Trim(),
            LogoUrl = logoUrl?.Trim(),
            MaxMembers = visibility == FantasyLeagueVisibility.Global ? null : (maxMembers ?? Rules.MaxClubMembers),
            JoinCode = joinCode?.Trim(),
            Status = PredictClubStatus.Draft,
            ExactScorePoints = exactScorePoints,
            OutcomePoints = outcomePoints,
            HalfTimeScorePoints = halfTimeScorePoints,
            BothTeamsToScorePoints = bothTeamsToScorePoints,
            OverUnderPoints = overUnderPoints,
            YellowCardsPoints = yellowCardsPoints,
            CornersPoints = cornersPoints,
            MatchMvpPoints = matchMvpPoints,
            FirstGoalMinutePoints = firstGoalMinutePoints,
            EnableTournamentPredictions = enableTournamentPredictions,
            TournamentPodiumPoints = tournamentPodiumPoints,
            TournamentAwardPoints = tournamentAwardPoints,
            PredictExactScore = predictExactScore,
            PredictOutcome = predictOutcome,
            PredictHalfTime = predictHalfTime,
            PredictBtts = predictBtts,
            PredictCards = predictCards,
            PredictCorners = predictCorners,
            PredictMvp = predictMvp
        };
    }

    public void OpenClub()
    {
        if (Status != PredictClubStatus.Draft)
            throw new InvalidOperationException("Only draft clubs can be opened.");
        Status = PredictClubStatus.Open;
    }

    public void ActivateClub()
    {
        if (Status != PredictClubStatus.Open)
            throw new InvalidOperationException("Only open clubs can become active.");
        Status = PredictClubStatus.Active;
    }

    public bool CanModifyRules(DateTime? checkDate = null)
    {
        var now = checkDate ?? DateTime.UtcNow;
        return Status == PredictClubStatus.Draft || (Status == PredictClubStatus.Open && now < StartsAt);
    }

    public void AssertCanModifyRules(DateTime? checkDate = null)
    {
        if (!CanModifyRules(checkDate))
        {
            throw new InvalidOperationException("Fantasy Predict Club rules cannot be changed after the club has started.");
        }
    }
}
