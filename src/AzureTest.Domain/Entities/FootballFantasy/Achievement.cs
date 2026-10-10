using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Achievement : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int TitleMinLength = 3;
        public const int TitleMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int BadgeIconMaxLength = 100;
        public const int MinPoints = 0;
        public const int MaxPoints = 10000;
    }

    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string BadgeIcon { get; private set; } = default!;
    public int PointsReward { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<UserAchievement> UserAchievements { get; private set; } = new List<UserAchievement>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Achievement() { }

    public static Achievement Create(string title, string description, string badgeIcon, int pointsReward)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > Rules.TitleMaxLength)
            throw new ArgumentException("Invalid achievement title.", nameof(title));
        if (string.IsNullOrWhiteSpace(description) || description.Length > Rules.DescriptionMaxLength)
            throw new ArgumentException("Invalid achievement description.", nameof(description));
        if (pointsReward < Rules.MinPoints || pointsReward > Rules.MaxPoints)
            throw new ArgumentException("Points reward out of allowed range.", nameof(pointsReward));

        return new Achievement
        {
            Title = title.Trim(),
            Description = description.Trim(),
            BadgeIcon = string.IsNullOrWhiteSpace(badgeIcon) ? "bi-trophy" : badgeIcon.Trim(),
            PointsReward = pointsReward,
            IsActive = true
        };
    }

    public void Update(string title, string description, string badgeIcon, int pointsReward, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > Rules.TitleMaxLength)
            throw new ArgumentException("Invalid achievement title.", nameof(title));
        if (string.IsNullOrWhiteSpace(description) || description.Length > Rules.DescriptionMaxLength)
            throw new ArgumentException("Invalid achievement description.", nameof(description));
        if (pointsReward < Rules.MinPoints || pointsReward > Rules.MaxPoints)
            throw new ArgumentException("Points reward out of allowed range.", nameof(pointsReward));

        Title = title.Trim();
        Description = description.Trim();
        BadgeIcon = string.IsNullOrWhiteSpace(badgeIcon) ? "bi-trophy" : badgeIcon.Trim();
        PointsReward = pointsReward;
        IsActive = isActive;
    }
}
