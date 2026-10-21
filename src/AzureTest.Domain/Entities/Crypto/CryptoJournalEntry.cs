using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoJournalEntry : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMaxLength = 20;
        public const int TagMaxLength = 50;
        public const int EmotionMaxLength = 50;
        public const int NotesMaxLength = 2000;
        public const int LessonsMaxLength = 2000;
        public const int RatingMin = 1;
        public const int RatingMax = 5;
    }

    public Guid? PositionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public string StrategyTag { get; private set; } = string.Empty;
    public string EmotionTag { get; private set; } = string.Empty;
    public int Rating { get; private set; } = 3;
    public string Notes { get; private set; } = string.Empty;
    public string LessonsLearned { get; private set; } = string.Empty;

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoJournalEntry() { }

    private CryptoJournalEntry(
        Guid? positionId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string strategyTag,
        string emotionTag,
        int rating,
        string notes,
        string lessonsLearned)
    {
        Id = Guid.NewGuid();
        PositionId = positionId;
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol;
        StrategyTag = strategyTag;
        EmotionTag = emotionTag;
        Rating = Math.Clamp(rating, Rules.RatingMin, Rules.RatingMax);
        Notes = notes;
        LessonsLearned = lessonsLearned;
    }

    public static CryptoJournalEntry Create(
        Guid? positionId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string? strategyTag = null,
        string? emotionTag = null,
        int rating = 3,
        string? notes = null,
        string? lessonsLearned = null)
    {
        return new CryptoJournalEntry(
            positionId,
            userId,
            tenantId,
            symbol.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(strategyTag) ? "General" : strategyTag.Trim(),
            string.IsNullOrWhiteSpace(emotionTag) ? "Neutral" : emotionTag.Trim(),
            rating,
            notes?.Trim() ?? string.Empty,
            lessonsLearned?.Trim() ?? string.Empty);
    }

    public void Update(string strategyTag, string emotionTag, int rating, string notes, string lessonsLearned)
    {
        StrategyTag = string.IsNullOrWhiteSpace(strategyTag) ? "General" : strategyTag.Trim();
        EmotionTag = string.IsNullOrWhiteSpace(emotionTag) ? "Neutral" : emotionTag.Trim();
        Rating = Math.Clamp(rating, Rules.RatingMin, Rules.RatingMax);
        Notes = notes.Trim();
        LessonsLearned = lessonsLearned.Trim();
    }
}
