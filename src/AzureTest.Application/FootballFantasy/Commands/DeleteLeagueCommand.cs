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

public sealed record DeleteLeagueCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteLeagueCommandHandler : BaseCommandHandler<DeleteLeagueCommand, Result>
{
    public DeleteLeagueCommandHandler(IUnitOfWork uow, ILogger<DeleteLeagueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.Leagues.GetByIdAsync(request.Id);
        if (league == null) return Result.Failure("League not found.");

        UnitOfWork.Leagues.Remove(league);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
