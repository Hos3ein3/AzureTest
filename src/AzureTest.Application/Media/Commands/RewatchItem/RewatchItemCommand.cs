using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.RewatchItem;

public enum RewatchTargetScope
{
    EntireTitle = 0,
    Season = 1,
    Episode = 2
}

public sealed record RewatchItemCommand(
    Guid UserId,
    Guid WatchItemId,
    bool RestartWatching = false,
    RewatchTargetScope Scope = RewatchTargetScope.EntireTitle,
    int? TargetSeason = null,
    int? TargetEpisode = null,
    bool IsUndo = false) : ICommand<Result>;

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

        if (cmd.IsUndo)
        {
            switch (cmd.Scope)
            {
                case RewatchTargetScope.Season:
                    if (cmd.TargetSeason.HasValue && cmd.TargetSeason.Value > 0)
                    {
                        item.UndoSeasonRewatch(cmd.TargetSeason.Value);
                    }
                    break;

                case RewatchTargetScope.Episode:
                    if (cmd.TargetEpisode.HasValue && cmd.TargetEpisode.Value > 0)
                    {
                        var s = cmd.TargetSeason ?? 1;
                        item.UndoEpisodeRewatch(s, cmd.TargetEpisode.Value);
                    }
                    break;

                case RewatchTargetScope.EntireTitle:
                default:
                    item.UndoRewatch();
                    break;
            }
        }
        else
        {
            switch (cmd.Scope)
            {
                case RewatchTargetScope.Season:
                    if (cmd.TargetSeason.HasValue && cmd.TargetSeason.Value > 0)
                    {
                        item.RewatchSeason(cmd.TargetSeason.Value, cmd.RestartWatching);
                    }
                    break;

                case RewatchTargetScope.Episode:
                    if (cmd.TargetEpisode.HasValue && cmd.TargetEpisode.Value > 0)
                    {
                        var s = cmd.TargetSeason ?? 1;
                        item.RewatchEpisode(s, cmd.TargetEpisode.Value, cmd.RestartWatching);
                    }
                    break;

                case RewatchTargetScope.EntireTitle:
                default:
                    item.IncrementRewatch(cmd.RestartWatching);
                    break;
            }
        }

        await _uow.WatchItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}
