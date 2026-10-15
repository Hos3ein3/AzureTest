using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetBookmakersQuery(BookmakerFilter Filter) : IQuery<PagedResult<BookmakerDto, BookmakerFilter>>;

public sealed class GetBookmakersQueryHandler : BaseQueryHandler<GetBookmakersQuery, PagedResult<BookmakerDto, BookmakerFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetBookmakersQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetBookmakersQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<BookmakerDto, BookmakerFilter>> ExecuteAsync(GetBookmakersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new BookmakerFilter();
        var all = await _uow.Bookmakers.GetAllAsync();
        var odds = await _uow.FixtureOdds.GetAllAsync();
        var oddsCount = odds.GroupBy(x => x.BookmakerId).ToDictionary(g => g.Key, g => g.Count());

        IEnumerable<Bookmaker> query = all;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(s));
        }

        var list = query.OrderBy(b => b.Name).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(b => new BookmakerDto(b.Id, b.Name, b.Url, oddsCount.GetValueOrDefault(b.Id, 0)))
            .ToList();

        return new PagedResult<BookmakerDto, BookmakerFilter>(items, total, page, size, filter);
    }
}
