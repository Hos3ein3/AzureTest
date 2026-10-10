using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

// CREATE ACHIEVEMENT
public sealed record CreateAchievementCommand(
    string Title,
    string Description,
    string BadgeIcon,
    int PointsReward
) : ICommand<Result<Guid>>;

public sealed class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    public CreateAchievementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(Achievement.Rules.TitleMinLength)
            .MaximumLength(Achievement.Rules.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(Achievement.Rules.DescriptionMaxLength);

        RuleFor(x => x.PointsReward)
            .InclusiveBetween(Achievement.Rules.MinPoints, Achievement.Rules.MaxPoints);
    }
}

public sealed class CreateAchievementCommandHandler : BaseCommandHandler<CreateAchievementCommand, Result<Guid>>
{
    public CreateAchievementCommandHandler(IUnitOfWork uow, ILogger<CreateAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateAchievementCommand cmd, CancellationToken cancellationToken)
    {
        var achievement = Achievement.Create(cmd.Title, cmd.Description, cmd.BadgeIcon, cmd.PointsReward);
        UnitOfWork.Achievements.Add(achievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(achievement.Id);
    }
}

// UPDATE ACHIEVEMENT
public sealed record UpdateAchievementCommand(
    Guid Id,
    string Title,
    string Description,
    string BadgeIcon,
    int PointsReward,
    bool IsActive
) : ICommand<Result>;

public sealed class UpdateAchievementCommandValidator : AbstractValidator<UpdateAchievementCommand>
{
    public UpdateAchievementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(Achievement.Rules.TitleMinLength)
            .MaximumLength(Achievement.Rules.TitleMaxLength);
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(Achievement.Rules.DescriptionMaxLength);
        RuleFor(x => x.PointsReward)
            .InclusiveBetween(Achievement.Rules.MinPoints, Achievement.Rules.MaxPoints);
    }
}

public sealed class UpdateAchievementCommandHandler : BaseCommandHandler<UpdateAchievementCommand, Result>
{
    public UpdateAchievementCommandHandler(IUnitOfWork uow, ILogger<UpdateAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateAchievementCommand cmd, CancellationToken cancellationToken)
    {
        var achievement = await UnitOfWork.Achievements.GetByIdAsync(cmd.Id);
        if (achievement == null) return Result.Failure("Achievement not found.");

        achievement.Update(cmd.Title, cmd.Description, cmd.BadgeIcon, cmd.PointsReward, cmd.IsActive);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// DELETE ACHIEVEMENT
public sealed record DeleteAchievementCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteAchievementCommandValidator : AbstractValidator<DeleteAchievementCommand>
{
    public DeleteAchievementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeleteAchievementCommandHandler : BaseCommandHandler<DeleteAchievementCommand, Result>
{
    public DeleteAchievementCommandHandler(IUnitOfWork uow, ILogger<DeleteAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteAchievementCommand cmd, CancellationToken cancellationToken)
    {
        var achievement = await UnitOfWork.Achievements.GetByIdAsync(cmd.Id);
        if (achievement == null) return Result.Failure("Achievement not found.");

        UnitOfWork.Achievements.Remove(achievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// UNLOCK USER ACHIEVEMENT
public sealed record UnlockUserAchievementCommand(Guid UserId, Guid AchievementId) : ICommand<Result<Guid>>;

public sealed class UnlockUserAchievementCommandHandler : BaseCommandHandler<UnlockUserAchievementCommand, Result<Guid>>
{
    public UnlockUserAchievementCommandHandler(IUnitOfWork uow, ILogger<UnlockUserAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(UnlockUserAchievementCommand cmd, CancellationToken cancellationToken)
    {
        var existing = await UnitOfWork.UserAchievements.FindAsync(x => x.UserId == cmd.UserId && x.AchievementId == cmd.AchievementId);
        if (existing.Count > 0) return Result<Guid>.Success(existing[0].Id);

        var achievement = await UnitOfWork.Achievements.GetByIdAsync(cmd.AchievementId);
        if (achievement == null || !achievement.IsActive) return Result<Guid>.Failure("Achievement does not exist or is inactive.");

        var userAchievement = UserAchievement.Create(cmd.UserId, cmd.AchievementId);
        UnitOfWork.UserAchievements.Add(userAchievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(userAchievement.Id);
    }
}
