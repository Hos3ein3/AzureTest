using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.AddItemToCustomList;

public sealed record AddItemToCustomListCommand(
    Guid UserId,
    Guid ListId,
    int TmdbId,
    MediaType Type,
    string Title,
    string? PosterPath = null,
    int? Year = null) : ICommand<Result>;

public sealed class AddItemToCustomListCommandValidator : AbstractValidator<AddItemToCustomListCommand>
{
    public AddItemToCustomListCommandValidator()
    {
        RuleFor(x => x.ListId).NotEmpty().WithMessage("List ID is required.");
        RuleFor(x => x.TmdbId).GreaterThan(0).WithMessage("Valid TMDb ID is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(WatchItemDocument.Rules.TitleMaxLength);
    }
}

public sealed class AddItemToCustomListCommandHandler : ICommandHandler<AddItemToCustomListCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public AddItemToCustomListCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(AddItemToCustomListCommand cmd, CancellationToken ct = default)
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

        var added = list.AddItem(cmd.TmdbId, cmd.Type, cmd.Title, cmd.PosterPath, cmd.Year);
        if (!added)
        {
            return Result.Failure("Item is already in this list or limit reached.");
        }

        await _uow.CustomMediaLists.UpdateAsync(list.Id, list, ct);
        return Result.Success();
    }
}
