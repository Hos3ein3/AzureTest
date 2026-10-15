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

public sealed record DeleteFixtureCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteFixtureCommandHandler : BaseCommandHandler<DeleteFixtureCommand, Result>
{
    public DeleteFixtureCommandHandler(IUnitOfWork uow, ILogger<DeleteFixtureCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        UnitOfWork.Fixtures.Remove(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
