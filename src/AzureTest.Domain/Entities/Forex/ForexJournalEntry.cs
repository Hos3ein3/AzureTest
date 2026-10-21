using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexJournalEntry : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMaxLength = 20;
        public const int SetupTypeMaxLength = 100;
        public const int SessionMaxLength = 50;
        public const int EmotionsMaxLength = 500;
        public const int MistakesMaxLength = 1000;
        public const int LessonsMaxLength = 2000;
        public const int NotesMaxLength = 3000;
        public const int UrlMaxLength = 500;
        public const int RatingMin = 1;
        public const int RatingMax = 5;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public Guid? TradeId { get; private set; }
    public string Symbol { get; private set; } = "EUR/USD";
    public DateTime TradeDateUtc { get; private set; }
    public string MarketSession { get; private set; } = "London";
    public string SetupType { get; private set; } = string.Empty;
    public string Emotions { get; private set; } = string.Empty;
    public string Mistakes { get; private set; } = string.Empty;
    public string LessonsLearned { get; private set; } = string.Empty;
    public string DetailedNotes { get; private set; } = string.Empty;
    public string? ScreenshotUrl { get; private set; }
    public int ExecutionRating { get; private set; } = 3;
    public bool FollowedPlan { get; private set; } = true;

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexJournalEntry() { }

    private ForexJournalEntry(
        Guid userId,
        Guid? tradeId,
        string symbol,
        DateTime tradeDateUtc,
        string marketSession,
        string setupType,
        string emotions,
        string mistakes,
        string lessonsLearned,
        string detailedNotes,
        string? screenshotUrl,
        int executionRating,
        bool followedPlan,
        Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        TradeId = tradeId;
        Symbol = symbol.ToUpperInvariant().Trim();
        TradeDateUtc = tradeDateUtc;
        MarketSession = marketSession.Trim();
        SetupType = setupType.Trim();
        Emotions = emotions.Trim();
        Mistakes = mistakes.Trim();
        LessonsLearned = lessonsLearned.Trim();
        DetailedNotes = detailedNotes.Trim();
        ScreenshotUrl = screenshotUrl?.Trim();
        ExecutionRating = Math.Clamp(executionRating, Rules.RatingMin, Rules.RatingMax);
        FollowedPlan = followedPlan;
    }

    public static ForexJournalEntry Create(
        Guid userId,
        Guid? tradeId,
        string symbol,
        DateTime tradeDateUtc,
        string marketSession,
        string setupType,
        string emotions,
        string mistakes,
        string lessonsLearned,
        string detailedNotes,
        string? screenshotUrl = null,
        int executionRating = 3,
        bool followedPlan = true,
        Guid? tenantId = null)
    {
        return new ForexJournalEntry(
            userId, tradeId, symbol, tradeDateUtc, marketSession, setupType, emotions,
            mistakes, lessonsLearned, detailedNotes, screenshotUrl, executionRating, followedPlan, tenantId);
    }

    public void Update(
        string setupType,
        string emotions,
        string mistakes,
        string lessonsLearned,
        string detailedNotes,
        string? screenshotUrl,
        int executionRating,
        bool followedPlan)
    {
        SetupType = setupType.Trim();
        Emotions = emotions.Trim();
        Mistakes = mistakes.Trim();
        LessonsLearned = lessonsLearned.Trim();
        DetailedNotes = detailedNotes.Trim();
        ScreenshotUrl = screenshotUrl?.Trim();
        ExecutionRating = Math.Clamp(executionRating, Rules.RatingMin, Rules.RatingMax);
        FollowedPlan = followedPlan;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
