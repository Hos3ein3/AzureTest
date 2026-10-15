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

public sealed record GetLeagueStandingsQuery(LeagueStandingFilter Filter) : IQuery<PagedResult<LeagueStandingDto, LeagueStandingFilter>>;

public sealed class GetLeagueStandingsQueryHandler : BaseQueryHandler<GetLeagueStandingsQuery, PagedResult<LeagueStandingDto, LeagueStandingFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetLeagueStandingsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetLeagueStandingsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<LeagueStandingDto, LeagueStandingFilter>> ExecuteAsync(GetLeagueStandingsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new LeagueStandingFilter();
        var all = await _uow.LeagueStandings.GetAllAsync();
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(x => x.Id, x => x);
        var seasons = (await _uow.Seasons.GetAllAsync()).ToDictionary(x => x.Id, x => x);

        IEnumerable<LeagueStanding> query = all;
        if (filter.SeasonId.HasValue) query = query.Where(x => x.SeasonId == filter.SeasonId.Value);
        if (filter.TeamId.HasValue) query = query.Where(x => x.TeamId == filter.TeamId.Value);

        var list = query.OrderBy(x => x.Rank).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(s => new LeagueStandingDto(
                s.Id,
                s.SeasonId,
                seasons.TryGetValue(s.SeasonId, out var sea) ? sea.Name : string.Empty,
                s.TeamId,
                teams.TryGetValue(s.TeamId, out var tm) ? tm.Name : string.Empty,
                teams.TryGetValue(s.TeamId, out var tm2) ? tm2.LogoUrl : null,
                s.Rank,
                s.Points,
                s.GoalsDiff,
                s.Form,
                s.Status,
                s.Description,
                s.Played,
                s.Won,
                s.Drawn,
                s.Lost,
                s.GoalsFor,
                s.GoalsAgainst
            )).ToList();

        return new PagedResult<LeagueStandingDto, LeagueStandingFilter>(items, total, page, size, filter);
    }
}
