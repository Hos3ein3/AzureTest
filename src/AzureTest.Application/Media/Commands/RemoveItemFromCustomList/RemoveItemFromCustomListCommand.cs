using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Media.Enums;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.RemoveItemFromCustomList;

public sealed record RemoveItemFromCustomListCommand(
    Guid UserId,
    Guid ListId,
    int TmdbId,
    MediaType Type) : ICommand<Result>;

public sealed class RemoveItemFromCustomListCommandValidator : AbstractValidator<RemoveItemFromCustomListCommand>
{
    public RemoveItemFromCustomListCommandValidator()
    {
        RuleFor(x => x.ListId).NotEmpty().WithMessage("List ID is required.");
        RuleFor(x => x.TmdbId).GreaterThan(0).WithMessage("Valid TMDb ID is required.");
    }
}

public sealed class RemoveItemFromCustomListCommandHandler : ICommandHandler<RemoveItemFromCustomListCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public RemoveItemFromCustomListCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(RemoveItemFromCustomListCommand cmd, CancellationToken ct = default)
    {
        var list = await _uow.CustomMediaLists.GetByIdAsync(cmd.ListId, ct);
        if (list == null)
        {
            return Result.Failure("Custom list not found.");
        }

        if (list.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to modify this list.");
        }

        var removed = list.RemoveItem(cmd.TmdbId, cmd.Type);
        if (!removed)
        {
            return Result.Failure("Item not found in this list.");
        }

        await _uow.CustomMediaLists.UpdateAsync(list.Id, list, ct);
        return Result.Success();
    }
}
