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

public sealed record DeletePlayerMatchStatCommand(Guid Id) : ICommand<Result>;

public sealed class DeletePlayerMatchStatCommandHandler : BaseCommandHandler<DeletePlayerMatchStatCommand, Result>
{
    public DeletePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<DeletePlayerMatchStatCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeletePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var stat = await UnitOfWork.PlayerMatchStats.GetByIdAsync(request.Id);
        if (stat == null) return Result.Failure("PlayerMatchStat not found.");

        UnitOfWork.PlayerMatchStats.Remove(stat);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
