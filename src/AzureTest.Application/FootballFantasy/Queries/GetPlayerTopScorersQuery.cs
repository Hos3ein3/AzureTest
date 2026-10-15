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

public sealed record GetPlayerTopScorersQuery(PlayerTopScorerFilter Filter) : IQuery<PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>>;

public sealed class GetPlayerTopScorersQueryHandler : BaseQueryHandler<GetPlayerTopScorersQuery, PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayerTopScorersQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayerTopScorersQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>> ExecuteAsync(GetPlayerTopScorersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerTopScorerFilter();
        var all = await _uow.PlayerTopScorers.GetAllAsync();
        var seasons = (await _uow.Seasons.GetAllAsync()).ToDictionary(s => s.Id, s => s.Name);
        var players = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.Id, p => p.DisplayName);
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Id, t => t.Name);

        IEnumerable<PlayerTopScorer> query = all;
        if (filter.SeasonId.HasValue) query = query.Where(x => x.SeasonId == filter.SeasonId.Value);

        var list = query.OrderBy(x => x.Rank).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(s => new PlayerTopScorerDto(
                s.Id,
                s.SeasonId,
                seasons.GetValueOrDefault(s.SeasonId, "Season"),
                s.PlayerId,
                players.GetValueOrDefault(s.PlayerId, "Player"),
                s.TeamId,
                teams.GetValueOrDefault(s.TeamId, "Team"),
                s.Rank,
                s.Goals,
                s.Assists ?? 0,
                s.PenaltyGoals ?? 0,
                s.MatchesPlayed ?? 0
            )).ToList();

        return new PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>(items, total, page, size, filter);
    }
}
