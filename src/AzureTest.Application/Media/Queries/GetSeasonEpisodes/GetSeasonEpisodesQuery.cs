using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Queries.GetSeasonEpisodes;

public sealed record GetSeasonEpisodesQuery(int TvShowId, int SeasonNumber) : IQuery<Result<IReadOnlyList<TvEpisodeSummaryDto>>>;

public sealed class GetSeasonEpisodesQueryValidator : AbstractValidator<GetSeasonEpisodesQuery>
{
    public GetSeasonEpisodesQueryValidator()
    {
        RuleFor(x => x.TvShowId)
            .GreaterThan(0).WithMessage("Valid TV show TMDb ID is required.");

        RuleFor(x => x.SeasonNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Season number cannot be negative.");
    }
}

public sealed class GetSeasonEpisodesQueryHandler : IQueryHandler<GetSeasonEpisodesQuery, Result<IReadOnlyList<TvEpisodeSummaryDto>>>
{
    private readonly ITmdbService _tmdbService;

    public GetSeasonEpisodesQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IReadOnlyList<TvEpisodeSummaryDto>>> HandleAsync(GetSeasonEpisodesQuery request, CancellationToken ct = default)
    {
        var episodes = await _tmdbService.GetTvSeasonEpisodesAsync(request.TvShowId, request.SeasonNumber, ct);
        return Result.Success(episodes);
    }
}
