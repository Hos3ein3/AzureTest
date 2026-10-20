using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.SearchMedia;

public sealed record SearchMediaQuery(string Query, string SearchType = "all", int Page = 1) : IQuery<Result<IReadOnlyList<TmdbSearchResultDto>>>;

public sealed class SearchMediaQueryHandler : IQueryHandler<SearchMediaQuery, Result<IReadOnlyList<TmdbSearchResultDto>>>
{
    private readonly ITmdbService _tmdbService;

    public SearchMediaQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IReadOnlyList<TmdbSearchResultDto>>> HandleAsync(SearchMediaQuery request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Result.Success<IReadOnlyList<TmdbSearchResultDto>>(Array.Empty<TmdbSearchResultDto>());
        }

        var results = new List<TmdbSearchResultDto>();

        if (string.Equals(request.SearchType, "people", StringComparison.OrdinalIgnoreCase))
        {
            var people = await _tmdbService.SearchPeopleAsync(request.Query, request.Page, ct);
            results.AddRange(people);
        }
        else if (string.Equals(request.SearchType, "titles", StringComparison.OrdinalIgnoreCase))
        {
            var titles = await _tmdbService.SearchTitlesAsync(request.Query, request.Page, ct);
            results.AddRange(titles);
        }
        else
        {
            var titleTask = _tmdbService.SearchTitlesAsync(request.Query, request.Page, ct);
            var peopleTask = _tmdbService.SearchPeopleAsync(request.Query, request.Page, ct);

            await Task.WhenAll(titleTask, peopleTask);

            results.AddRange(titleTask.Result);
            results.AddRange(peopleTask.Result);
        }

        return Result.Success<IReadOnlyList<TmdbSearchResultDto>>(results);
    }
}
