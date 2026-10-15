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

public sealed record CreateAchievementCommand(
    string Title,
    string Description,
    string BadgeIcon,
    int PointsReward,
    bool IsActive = true
) : ICommand<Result<Guid>>;

public sealed class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    public CreateAchievementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Achievement.Rules.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(Achievement.Rules.DescriptionMaxLength);
        RuleFor(x => x.BadgeIcon).NotEmpty().MaximumLength(Achievement.Rules.BadgeIconMaxLength);
        RuleFor(x => x.PointsReward).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateAchievementCommandHandler : BaseCommandHandler<CreateAchievementCommand, Result<Guid>>
{
    public CreateAchievementCommandHandler(IUnitOfWork uow, ILogger<CreateAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = Achievement.Create(
            request.Title,
            request.Description,
            request.BadgeIcon,
            request.PointsReward
        );

        UnitOfWork.Achievements.Add(achievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(achievement.Id);
    }
}
