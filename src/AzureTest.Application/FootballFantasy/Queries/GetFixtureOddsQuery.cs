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

public sealed record GetFixtureOddsQuery(FixtureOddFilter Filter) : IQuery<PagedResult<FixtureOddDto, FixtureOddFilter>>;

public sealed class GetFixtureOddsQueryHandler : BaseQueryHandler<GetFixtureOddsQuery, PagedResult<FixtureOddDto, FixtureOddFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetFixtureOddsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetFixtureOddsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<FixtureOddDto, FixtureOddFilter>> ExecuteAsync(GetFixtureOddsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new FixtureOddFilter();
        var all = await _uow.FixtureOdds.GetAllAsync();
        var bookmakers = (await _uow.Bookmakers.GetAllAsync()).ToDictionary(b => b.Id, b => b.Name);
        var fixtures = (await _uow.Fixtures.GetAllAsync()).ToDictionary(f => f.Id, f => $"{f.Round} ({f.KickoffAt:yyyy-MM-dd})");

        IEnumerable<FixtureOdd> query = all;
        if (filter.FixtureId.HasValue) query = query.Where(x => x.FixtureId == filter.FixtureId.Value);
        if (filter.BookmakerId.HasValue) query = query.Where(x => x.BookmakerId == filter.BookmakerId.Value);
        if (filter.MarketType.HasValue) query = query.Where(x => x.MarketType == filter.MarketType.Value);
        if (filter.IsLive.HasValue) query = query.Where(x => x.IsLive == filter.IsLive.Value);

        var list = query.OrderByDescending(x => x.RecordedAt).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(o => new FixtureOddDto(
                o.Id,
                o.FixtureId,
                fixtures.GetValueOrDefault(o.FixtureId, "Fixture"),
                o.BookmakerId,
                bookmakers.GetValueOrDefault(o.BookmakerId, "Bookmaker"),
                o.MarketType,
                o.ValueLabel,
                o.Price,
                o.IsLive,
                o.RecordedAt
            )).ToList();

        return new PagedResult<FixtureOddDto, FixtureOddFilter>(items, total, page, size, filter);
    }
}
