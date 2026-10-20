using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.GetMovieNews;

public sealed record GetMovieNewsQuery(int Count = 20) : IQuery<Result<IReadOnlyList<MovieNewsItemDto>>>;

public sealed class GetMovieNewsQueryHandler : IQueryHandler<GetMovieNewsQuery, Result<IReadOnlyList<MovieNewsItemDto>>>
{
    private readonly IMovieNewsService _newsService;

    public GetMovieNewsQueryHandler(IMovieNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<Result<IReadOnlyList<MovieNewsItemDto>>> HandleAsync(GetMovieNewsQuery request, CancellationToken ct = default)
    {
        var count = Math.Clamp(request.Count, 1, 50);
        var news = await _newsService.GetLatestNewsAsync(count, ct);
        return Result.Success(news);
    }
}
