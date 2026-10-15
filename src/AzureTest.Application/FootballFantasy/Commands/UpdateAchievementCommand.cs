using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

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
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Achievement.Rules.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(Achievement.Rules.DescriptionMaxLength);
        RuleFor(x => x.BadgeIcon).NotEmpty().MaximumLength(Achievement.Rules.BadgeIconMaxLength);
        RuleFor(x => x.PointsReward).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateAchievementCommandHandler : BaseCommandHandler<UpdateAchievementCommand, Result>
{
    public UpdateAchievementCommandHandler(IUnitOfWork uow, ILogger<UpdateAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await UnitOfWork.Achievements.GetByIdAsync(request.Id);
        if (achievement == null) return Result.Failure("Achievement not found.");

        achievement.Update(request.Title, request.Description, request.BadgeIcon, request.PointsReward, request.IsActive);
        UnitOfWork.Achievements.Update(achievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
