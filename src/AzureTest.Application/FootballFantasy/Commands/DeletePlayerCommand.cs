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

public sealed record DeletePlayerCommand(Guid Id) : ICommand<Result>;

public sealed class DeletePlayerCommandHandler : BaseCommandHandler<DeletePlayerCommand, Result>
{
    public DeletePlayerCommandHandler(IUnitOfWork uow, ILogger<DeletePlayerCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await UnitOfWork.Players.GetByIdAsync(request.Id);
        if (player == null) return Result.Failure("Player not found.");

        UnitOfWork.Players.Remove(player);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
