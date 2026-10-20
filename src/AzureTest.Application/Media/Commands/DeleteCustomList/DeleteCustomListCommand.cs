using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.DeleteCustomList;

public sealed record DeleteCustomListCommand(
    Guid UserId,
    Guid ListId) : ICommand<Result>;

public sealed class DeleteCustomListCommandValidator : AbstractValidator<DeleteCustomListCommand>
{
    public DeleteCustomListCommandValidator()
    {
        RuleFor(x => x.ListId).NotEmpty().WithMessage("List ID is required.");
    }
}

public sealed class DeleteCustomListCommandHandler : ICommandHandler<DeleteCustomListCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public DeleteCustomListCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(DeleteCustomListCommand cmd, CancellationToken ct = default)
    {
        var list = await _uow.CustomMediaLists.GetByIdAsync(cmd.ListId, ct);
        if (list == null)
        {
            return Result.Failure("Custom list not found.");
        }

        if (list.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to delete this list.");
        }

        await _uow.CustomMediaLists.RemoveAsync(cmd.ListId, ct);
        return Result.Success();
    }
}
