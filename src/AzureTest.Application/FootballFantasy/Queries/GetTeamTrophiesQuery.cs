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

public sealed record GetTeamTrophiesQuery(TeamTrophyFilter Filter) : IQuery<PagedResult<TeamTrophyDto, TeamTrophyFilter>>;

public sealed class GetTeamTrophiesQueryHandler : BaseQueryHandler<GetTeamTrophiesQuery, PagedResult<TeamTrophyDto, TeamTrophyFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetTeamTrophiesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetTeamTrophiesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<TeamTrophyDto, TeamTrophyFilter>> ExecuteAsync(GetTeamTrophiesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new TeamTrophyFilter();
        var all = await _uow.TeamTrophies.GetAllAsync();
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Id, t => t.Name);
        var players = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.Id, p => p.DisplayName);
        var coaches = (await _uow.Coaches.GetAllAsync()).ToDictionary(c => c.Id, c => $"{c.FirstName} {c.LastName}".Trim());

        IEnumerable<TeamTrophy> query = all;
        if (filter.TeamId.HasValue) query = query.Where(x => x.TeamId == filter.TeamId.Value);
        if (filter.PlayerId.HasValue) query = query.Where(x => x.PlayerId == filter.PlayerId.Value);
        if (filter.CoachId.HasValue) query = query.Where(x => x.CoachId == filter.CoachId.Value);

        var list = query.OrderByDescending(x => x.Season).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(t => new TeamTrophyDto(
                t.Id,
                t.TeamId,
                t.TeamId.HasValue ? teams.GetValueOrDefault(t.TeamId.Value) : null,
                t.PlayerId,
                t.PlayerId.HasValue ? players.GetValueOrDefault(t.PlayerId.Value) : null,
                t.CoachId,
                t.CoachId.HasValue ? coaches.GetValueOrDefault(t.CoachId.Value) : null,
                t.LeagueName,
                t.Season,
                t.Place
            )).ToList();

        return new PagedResult<TeamTrophyDto, TeamTrophyFilter>(items, total, page, size, filter);
    }
}
