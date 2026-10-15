using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record UnlockUserAchievementCommand(Guid UserId, Guid AchievementId) : ICommand<Result<Guid>>;

public sealed class UnlockUserAchievementCommandHandler : BaseCommandHandler<UnlockUserAchievementCommand, Result<Guid>>
{
    public UnlockUserAchievementCommandHandler(IUnitOfWork uow, ILogger<UnlockUserAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(UnlockUserAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await UnitOfWork.Achievements.GetByIdAsync(request.AchievementId);
        if (achievement == null) return Result<Guid>.Failure("Achievement not found.");

        var existing = await UnitOfWork.UserAchievements.FindAsync(ua => ua.UserId == request.UserId && ua.AchievementId == request.AchievementId);
        if (existing.Count > 0) return Result<Guid>.Success(existing[0].Id);

        var userAchievement = UserAchievement.Create(request.UserId, request.AchievementId);
        UnitOfWork.UserAchievements.Add(userAchievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(userAchievement.Id);
    }
}
