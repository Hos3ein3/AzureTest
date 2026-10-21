using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.RewatchItem;

public sealed record RewatchItemCommand(
    Guid UserId,
    Guid WatchItemId,
    bool RestartWatching = false) : ICommand<Result>;

public sealed class RewatchItemCommandValidator : AbstractValidator<RewatchItemCommand>
{
    public RewatchItemCommandValidator()
    {
        RuleFor(x => x.WatchItemId).NotEmpty().WithMessage("Watch item ID is required.");
    }
}

public sealed class RewatchItemCommandHandler : ICommandHandler<RewatchItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public RewatchItemCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(RewatchItemCommand cmd, CancellationToken ct = default)
    {
        var item = await _uow.WatchItems.GetByIdAsync(cmd.WatchItemId, ct);
        if (item == null)
        {
            return Result.Failure("Watch item not found.");
        }

        if (item.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to update this watch item.");
        }

        item.IncrementRewatch(cmd.RestartWatching);
        await _uow.WatchItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}
