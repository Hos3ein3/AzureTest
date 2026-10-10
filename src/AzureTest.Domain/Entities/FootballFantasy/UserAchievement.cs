using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class UserAchievement : BaseEntity, IAudit
{
    public Guid UserId { get; private set; }
    public Guid AchievementId { get; private set; }
    public Achievement Achievement { get; private set; } = default!;
    public DateTime UnlockedAt { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private UserAchievement() { }

    public static UserAchievement Create(Guid userId, Guid achievementId)
    {
        return new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            UnlockedAt = DateTime.UtcNow
        };
    }
}
