using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.AddWatchItem;

public sealed record AddWatchItemCommand(
    Guid UserId,
    Guid TenantId,
    int TmdbId,
    MediaType Type,
    string Title,
    string? OriginalTitle = null,
    string? PosterPath = null,
    string? BackdropPath = null,
    string? Overview = null,
    DateTime? ReleaseDate = null,
    double? TmdbVoteAverage = null,
    WatchStatus Status = WatchStatus.PlanToWatch,
    int CurrentSeason = 1,
    int CurrentEpisode = 0,
    int TotalSeasons = 1,
    int TotalEpisodes = 1,
    int? RuntimeMinutes = null,
    List<string>? Genres = null,
    DateTime? WatchedDate = null,
    int? UserRating = null,
    string? UserReview = null) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static WatchItemDocument ToDomain(AddWatchItemCommand cmd) =>
            WatchItemDocument.Create(
                cmd.UserId,
                cmd.TenantId,
                cmd.TmdbId,
                cmd.Type,
                cmd.Title,
                cmd.OriginalTitle,
                cmd.PosterPath,
                cmd.BackdropPath,
                cmd.Overview,
                cmd.ReleaseDate,
                cmd.TmdbVoteAverage,
                cmd.Status,
                cmd.CurrentSeason,
                cmd.CurrentEpisode,
                cmd.TotalSeasons,
                cmd.TotalEpisodes,
                cmd.RuntimeMinutes,
                cmd.Genres,
                cmd.WatchedDate,
                cmd.UserRating,
                cmd.UserReview
            );
    }
}

public sealed class AddWatchItemCommandValidator : AbstractValidator<AddWatchItemCommand>
{
    public AddWatchItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(WatchItemDocument.Rules.TitleMaxLength);

        RuleFor(x => x.TmdbId)
            .GreaterThan(0).WithMessage("Valid TMDb ID is required.");

        RuleFor(x => x.CurrentSeason)
            .InclusiveBetween(WatchItemDocument.Rules.MinSeason, WatchItemDocument.Rules.MaxSeason);

        RuleFor(x => x.CurrentEpisode)
            .InclusiveBetween(WatchItemDocument.Rules.MinEpisode, WatchItemDocument.Rules.MaxEpisode);
    }
}

public sealed class AddWatchItemCommandHandler : ICommandHandler<AddWatchItemCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    private readonly ITmdbService _tmdbService;

    public AddWatchItemCommandHandler(INoSqlUnitOfWork uow, ITmdbService tmdbService)
    {
        _uow = uow;
        _tmdbService = tmdbService;
    }

    public async Task<Result<Guid>> HandleAsync(AddWatchItemCommand cmd, CancellationToken ct = default)
    {
        var existing = await _uow.WatchItems.GetByUserAndTmdbAsync(cmd.UserId, cmd.TmdbId, cmd.Type, ct);
        if (existing != null)
        {
            if (cmd.Status != existing.Status)
            {
                existing.ChangeStatus(cmd.Status);
            }
            var effectiveTotalSeasons = cmd.TotalSeasons > 1 ? cmd.TotalSeasons : existing.TotalSeasons;
            var effectiveTotalEpisodes = cmd.TotalEpisodes > 1 ? cmd.TotalEpisodes : existing.TotalEpisodes;
            if (cmd.Status != WatchStatus.PlanToWatch || cmd.CurrentSeason > 1 || cmd.CurrentEpisode >= 0)
            {
                existing.UpdateProgress(cmd.CurrentSeason, cmd.CurrentEpisode, effectiveTotalSeasons, effectiveTotalEpisodes);
            }
            if (cmd.WatchedDate.HasValue || cmd.UserRating.HasValue || !string.IsNullOrWhiteSpace(cmd.UserReview))
            {
                existing.RateAndReview(cmd.UserRating ?? existing.UserRating, cmd.UserReview ?? existing.UserReview, cmd.WatchedDate ?? existing.WatchedDate);
            }
            await _uow.WatchItems.UpdateAsync(existing.Id, existing, ct);
            return Result.Success(existing.Id);
        }

        var effectiveCmd = cmd;
        if (string.IsNullOrWhiteSpace(effectiveCmd.Title) || string.IsNullOrWhiteSpace(effectiveCmd.PosterPath))
        {
            var details = effectiveCmd.Type == MediaType.Movie
                ? await _tmdbService.GetMovieDetailsAsync(effectiveCmd.TmdbId, ct)
                : await _tmdbService.GetTvShowDetailsAsync(effectiveCmd.TmdbId, ct);

            if (details != null)
            {
                effectiveCmd = effectiveCmd with
                {
                    Title = string.IsNullOrWhiteSpace(effectiveCmd.Title) ? details.Title : effectiveCmd.Title,
                    OriginalTitle = details.OriginalTitle,
                    PosterPath = effectiveCmd.PosterPath ?? details.PosterPath,
                    BackdropPath = effectiveCmd.BackdropPath ?? details.BackdropPath,
                    Overview = effectiveCmd.Overview ?? details.Overview,
                    ReleaseDate = effectiveCmd.ReleaseDate ?? details.ReleaseDate,
                    TmdbVoteAverage = effectiveCmd.TmdbVoteAverage ?? details.VoteAverage,
                    RuntimeMinutes = effectiveCmd.RuntimeMinutes ?? details.RuntimeMinutes,
                    TotalSeasons = details.Seasons?.Count ?? effectiveCmd.TotalSeasons,
                    Genres = details.Genres ?? effectiveCmd.Genres
                };
            }
        }

        if (string.IsNullOrWhiteSpace(effectiveCmd.Title))
        {
            effectiveCmd = effectiveCmd with { Title = "Untitled Media" };
        }

        var document = AddWatchItemCommand.Mapping.ToDomain(effectiveCmd);
        await _uow.WatchItems.AddAsync(document, ct);
        return Result.Success(document.Id);
    }
}
