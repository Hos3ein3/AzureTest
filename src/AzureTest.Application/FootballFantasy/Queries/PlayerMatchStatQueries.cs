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
    private readonly IUnitOfWork _uow;

    public GetPlayerMatchStatsQueryHandler(IUnitOfWork uow, ILogger<GetPlayerMatchStatsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>> ExecuteAsync(GetPlayerMatchStatsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerMatchStatFilter();
        var all = await _uow.PlayerMatchStats.GetAllAsync();
        var players = await _uow.Players.GetAllAsync();
        var teams = await _uow.Teams.GetAllAsync();
        var fixtures = await _uow.Fixtures.GetAllAsync();

        var playerMap = players.ToDictionary(p => p.Id, p => p.DisplayName);
        var teamMap = teams.ToDictionary(t => t.Id, t => t.Name);
        var fixtureMap = fixtures.ToDictionary(f => f.Id, f => $"Match {f.KickoffAt:yyyy-MM-dd}");

        IEnumerable<PlayerMatchStat> query = all;

        if (filter.FixtureId.HasValue)
            query = query.Where(s => s.FixtureId == filter.FixtureId.Value);

        if (filter.PlayerId.HasValue)
            query = query.Where(s => s.PlayerId == filter.PlayerId.Value);

        if (filter.TeamId.HasValue)
            query = query.Where(s => s.TeamId == filter.TeamId.Value);

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderByDescending(s => s.MinutesPlayed)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(s => new PlayerMatchStatDto(
                s.Id,
                s.FixtureId,
                fixtureMap.TryGetValue(s.FixtureId, out var fName) ? fName : "Match",
                s.PlayerId,
                playerMap.TryGetValue(s.PlayerId, out var pName) ? pName : "Player",
                s.TeamId,
                teamMap.TryGetValue(s.TeamId, out var tName) ? tName : "Team",
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

public sealed record GetPlayerMatchStatByIdQuery(Guid Id) : IQuery<Result<PlayerMatchStatDto>>;

public sealed class GetPlayerMatchStatByIdQueryHandler : BaseQueryHandler<GetPlayerMatchStatByIdQuery, Result<PlayerMatchStatDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPlayerMatchStatByIdQueryHandler(IUnitOfWork uow, ILogger<GetPlayerMatchStatByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<PlayerMatchStatDto>> ExecuteAsync(GetPlayerMatchStatByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _uow.PlayerMatchStats.GetByIdAsync(request.Id);
        if (s == null) return Result<PlayerMatchStatDto>.Failure("Player match stat not found.");

        var player = await _uow.Players.GetByIdAsync(s.PlayerId);
        var team = await _uow.Teams.GetByIdAsync(s.TeamId);
        var fixture = await _uow.Fixtures.GetByIdAsync(s.FixtureId);

        return Result<PlayerMatchStatDto>.Success(new PlayerMatchStatDto(
            s.Id,
            s.FixtureId,
            $"Match {fixture?.KickoffAt:yyyy-MM-dd}",
            s.PlayerId,
            player?.DisplayName ?? "Player",
            s.TeamId,
            team?.Name ?? "Team",
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
        ));
    }
}
