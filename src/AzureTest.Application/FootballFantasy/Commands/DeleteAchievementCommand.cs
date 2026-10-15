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

public sealed record DeleteAchievementCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteAchievementCommandHandler : BaseCommandHandler<DeleteAchievementCommand, Result>
{
    public DeleteAchievementCommandHandler(IUnitOfWork uow, ILogger<DeleteAchievementCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await UnitOfWork.Achievements.GetByIdAsync(request.Id);
        if (achievement == null) return Result.Failure("Achievement not found.");

        UnitOfWork.Achievements.Remove(achievement);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
