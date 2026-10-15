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

public sealed record GetFixturesQuery(FixtureFilter Filter) : IQuery<PagedResult<FixtureDto, FixtureFilter>>;

public sealed class GetFixturesQueryHandler : BaseQueryHandler<GetFixturesQuery, PagedResult<FixtureDto, FixtureFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetFixturesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetFixturesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<FixtureDto, FixtureFilter>> ExecuteAsync(GetFixturesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new FixtureFilter();
        var all = await _uow.Fixtures.GetAllAsync();
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Id, t => t);

        IEnumerable<Fixture> query = all;
        if (filter.SeasonId.HasValue)
        {
            query = query.Where(f => f.SeasonId == filter.SeasonId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(f => f.Status == filter.Status.Value);
        }

        var list = query.OrderBy(f => f.KickoffAt).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(f => new FixtureDto(
                f.Id,
                f.SeasonId,
                teams.TryGetValue(f.HomeTeamId, out var ht) ? ht.Name : "Home",
                teams.TryGetValue(f.HomeTeamId, out var ht2) ? ht2.LogoUrl : null,
                teams.TryGetValue(f.AwayTeamId, out var at) ? at.Name : "Away",
                teams.TryGetValue(f.AwayTeamId, out var at2) ? at2.LogoUrl : null,
                f.KickoffAt,
                f.Status,
                f.HomeScore,
                f.AwayScore,
                f.Minute,
                f.Round,
                f.Venue,
                f.IsStatsFinalized
            ))
            .ToList();

        return new PagedResult<FixtureDto, FixtureFilter>(items, total, page, size, filter);
    }
}
