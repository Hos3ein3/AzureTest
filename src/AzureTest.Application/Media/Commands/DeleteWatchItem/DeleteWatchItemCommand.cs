using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.DeleteWatchItem;

public sealed record DeleteWatchItemCommand(
    Guid UserId,
    Guid WatchItemId) : ICommand<Result>;

public sealed class DeleteWatchItemCommandValidator : AbstractValidator<DeleteWatchItemCommand>
{
    public DeleteWatchItemCommandValidator()
    {
        RuleFor(x => x.WatchItemId)
            .NotEmpty().WithMessage("Watch item ID is required.");
    }
}

public sealed class DeleteWatchItemCommandHandler : ICommandHandler<DeleteWatchItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public DeleteWatchItemCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(DeleteWatchItemCommand cmd, CancellationToken ct = default)
    {
        var item = await _uow.WatchItems.GetByIdAsync(cmd.WatchItemId, ct);
        if (item == null)
        {
            return Result.Failure("Watch item not found.");
        }

        if (item.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to delete this watch item.");
        }

        await _uow.WatchItems.RemoveAsync(cmd.WatchItemId, ct);
        return Result.Success();
    }
}
