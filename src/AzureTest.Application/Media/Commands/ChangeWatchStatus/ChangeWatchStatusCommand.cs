using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Media.Enums;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.ChangeWatchStatus;

public sealed record ChangeWatchStatusCommand(
    Guid UserId,
    Guid WatchItemId,
    WatchStatus Status) : ICommand<Result>;

public sealed class ChangeWatchStatusCommandValidator : AbstractValidator<ChangeWatchStatusCommand>
{
    public ChangeWatchStatusCommandValidator()
    {
        RuleFor(x => x.WatchItemId)
            .NotEmpty().WithMessage("Watch item ID is required.");
    }
}

public sealed class ChangeWatchStatusCommandHandler : ICommandHandler<ChangeWatchStatusCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public ChangeWatchStatusCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(ChangeWatchStatusCommand cmd, CancellationToken ct = default)
    {
        var item = await _uow.WatchItems.GetByIdAsync(cmd.WatchItemId, ct);
        if (item == null)
        {
            return Result.Failure("Watch item not found.");
        }

        if (item.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to modify this watch item.");
        }

        item.ChangeStatus(cmd.Status);
        await _uow.WatchItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}
