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

public sealed record DeleteTeamCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteTeamCommandHandler : BaseCommandHandler<DeleteTeamCommand, Result>
{
    public DeleteTeamCommandHandler(IUnitOfWork uow, ILogger<DeleteTeamCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await UnitOfWork.Teams.GetByIdAsync(request.Id);
        if (team == null) return Result.Failure("Team not found.");

        UnitOfWork.Teams.Remove(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
