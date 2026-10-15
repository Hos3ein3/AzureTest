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

public sealed record GetPlayerMatchStatsQuery(PlayerMatchStatFilter Filter) : IQuery<PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>>;

public sealed class GetPlayerMatchStatsQueryHandler : BaseQueryHandler<GetPlayerMatchStatsQuery, PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayerMatchStatsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayerMatchStatsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>> ExecuteAsync(GetPlayerMatchStatsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerMatchStatFilter();
        var all = await _uow.PlayerMatchStats.GetAllAsync();
        var players = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.Id, p => p.DisplayName);
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Id, t => t.Name);
        var fixtures = (await _uow.Fixtures.GetAllAsync()).ToDictionary(f => f.Id, f => $"{f.Round ?? "Match"} ({f.KickoffAt:yyyy-MM-dd})");

        IEnumerable<PlayerMatchStat> query = all;
        if (filter.FixtureId.HasValue) query = query.Where(s => s.FixtureId == filter.FixtureId.Value);
        if (filter.PlayerId.HasValue) query = query.Where(s => s.PlayerId == filter.PlayerId.Value);
        if (filter.TeamId.HasValue) query = query.Where(s => s.TeamId == filter.TeamId.Value);

        var list = query.OrderByDescending(s => s.MinutesPlayed).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new PlayerMatchStatDto(
                s.Id,
                s.FixtureId,
                fixtures.GetValueOrDefault(s.FixtureId, "Fixture"),
                s.PlayerId,
                players.GetValueOrDefault(s.PlayerId, "Player"),
                s.TeamId,
                teams.GetValueOrDefault(s.TeamId, "Team"),
                s.IsStarter,
                s.MinutesPlayed,
                s.Goals,
                s.Assists,
                s.OwnGoals,
                s.ShotsTotal,
                s.ShotsOnTarget,
                s.PassesTotal,
                s.KeyPasses,
                s.Tackles,
                s.Interceptions,
                s.Saves,
                s.GoalsConceded,
                s.PenaltiesSaved,
                s.PenaltiesMissed,
                s.YellowCards,
                s.RedCards,
                s.CleanSheet,
                s.ProviderRating
            ))
            .ToList();

        return new PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>(items, total, page, size, filter);
    }
}
