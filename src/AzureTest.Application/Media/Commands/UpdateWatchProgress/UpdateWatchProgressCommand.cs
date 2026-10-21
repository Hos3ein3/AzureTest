using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.UpdateWatchProgress;

public sealed record UpdateWatchProgressCommand(
    Guid UserId,
    Guid WatchItemId,
    int Season,
    int Episode,
    int? TotalSeasons = null,
    int? TotalEpisodes = null) : ICommand<Result>;

public sealed class UpdateWatchProgressCommandValidator : AbstractValidator<UpdateWatchProgressCommand>
{
    public UpdateWatchProgressCommandValidator()
    {
        RuleFor(x => x.Season)
            .InclusiveBetween(WatchItemDocument.Rules.MinSeason, WatchItemDocument.Rules.MaxSeason);

        RuleFor(x => x.Episode)
            .InclusiveBetween(WatchItemDocument.Rules.MinEpisode, WatchItemDocument.Rules.MaxEpisode);
    }
}

public sealed class UpdateWatchProgressCommandHandler : ICommandHandler<UpdateWatchProgressCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public UpdateWatchProgressCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(UpdateWatchProgressCommand cmd, CancellationToken ct = default)
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

        var effectiveTotalSeasons = (cmd.TotalSeasons.HasValue && cmd.TotalSeasons.Value > 1) ? cmd.TotalSeasons.Value : item.TotalSeasons;
        var effectiveTotalEpisodes = (cmd.TotalEpisodes.HasValue && cmd.TotalEpisodes.Value > 1) ? cmd.TotalEpisodes.Value : item.TotalEpisodes;
        item.UpdateProgress(cmd.Season, cmd.Episode, effectiveTotalSeasons, effectiveTotalEpisodes);
        await _uow.WatchItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}
