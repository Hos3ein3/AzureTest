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

public sealed record DeleteCoachCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteCoachCommandHandler : BaseCommandHandler<DeleteCoachCommand, Result>
{
    public DeleteCoachCommandHandler(IUnitOfWork uow, ILogger<DeleteCoachCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = await UnitOfWork.Coaches.GetByIdAsync(request.Id);
        if (coach == null) return Result.Failure("Coach not found.");

        UnitOfWork.Coaches.Remove(coach);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
