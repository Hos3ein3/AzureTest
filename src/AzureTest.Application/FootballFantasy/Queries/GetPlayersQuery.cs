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
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPlayersQuery(PlayerFilter Filter) : IQuery<PagedResult<PlayerDto, PlayerFilter>>;

public sealed class GetPlayersQueryHandler : BaseQueryHandler<GetPlayersQuery, PagedResult<PlayerDto, PlayerFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayersQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayersQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerDto, PlayerFilter>> ExecuteAsync(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerFilter();
        var allPlayers = await _uow.Players.GetPlayersWithDetailsAsync(cancellationToken);
        IEnumerable<AzureTest.Domain.Entities.FootballFantasy.Player> query = allPlayers.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(p => (p.DisplayName != null && p.DisplayName.ToLower().Contains(search))
                                  || (p.LastName != null && p.LastName.ToLower().Contains(search)));
        }

        if (filter.FantasyLeagueId.HasValue)
        {
            var league = await _uow.FantasyLeagues.GetWithDetailsAsync(filter.FantasyLeagueId.Value, cancellationToken);
            if (league != null && league.PoolRules.Count > 0)
            {
                var poolRule = league.PoolRules.FirstOrDefault();
                if (poolRule != null && poolRule.ScopeType != AzureTest.Domain.Entities.FootballFantasy.Enums.PoolScopeType.All)
                {
                    var val = poolRule.ScopeValue;
                    if (poolRule.ScopeType == AzureTest.Domain.Entities.FootballFantasy.Enums.PoolScopeType.Country)
                    {
                        query = query.Where(p => (p.Country != null && (p.Country.Name.Equals(val, StringComparison.OrdinalIgnoreCase) || p.Country.Code.Equals(val, StringComparison.OrdinalIgnoreCase)))
                                              || (p.CountryId.ToString() == val));
                    }
                    else if (poolRule.ScopeType == AzureTest.Domain.Entities.FootballFantasy.Enums.PoolScopeType.Continent)
                    {
                        query = query.Where(p => (p.Country != null && p.Country.Continent.Equals(val, StringComparison.OrdinalIgnoreCase))
                                              || p.PlayerSquads.Any(ps => ps.ToDate == null && ps.TeamSeason != null && ps.TeamSeason.Season != null && ps.TeamSeason.Season.League != null && ps.TeamSeason.Season.League.Continent != null && ps.TeamSeason.Season.League.Continent.Equals(val, StringComparison.OrdinalIgnoreCase)));
                    }
                    else if (poolRule.ScopeType == AzureTest.Domain.Entities.FootballFantasy.Enums.PoolScopeType.League)
                    {
                        query = query.Where(p => p.PlayerSquads.Any(ps => ps.ToDate == null && ps.TeamSeason != null && ps.TeamSeason.Season != null && ps.TeamSeason.Season.League != null && (ps.TeamSeason.Season.League.Name.Equals(val, StringComparison.OrdinalIgnoreCase) || ps.TeamSeason.Season.League.ShortName == val || ps.TeamSeason.Season.LeagueId.ToString() == val)));
                    }
                }
            }
        }

        if (filter.Position.HasValue)
            query = query.Where(p => p.Position == filter.Position.Value);

        if (filter.TeamId.HasValue)
            query = query.Where(p => p.PlayerSquads.Any(ps => ps.TeamSeason != null && ps.TeamSeason.TeamId == filter.TeamId.Value && ps.ToDate == null));

        var playerList = query.ToList();
        var totalCount = playerList.Count;
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = playerList
            .OrderBy(p => p.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p =>
            {
                var teamName = p.PlayerSquads
                    .Where(ps => ps.ToDate == null)
                    .Select(ps => ps.TeamSeason?.Team?.ShortName ?? "Free Agent")
                    .FirstOrDefault() ?? "Free Agent";

                var latestPrice = p.Prices
                    .OrderByDescending(pr => pr.ValidFrom)
                    .Select(pr => pr.Price)
                    .FirstOrDefault();

                var price = latestPrice == 0 ? 5.0m : latestPrice;

                return new PlayerDto(
                    p.Id,
                    p.FirstName,
                    p.LastName,
                    p.DisplayName,
                    p.DateOfBirth,
                    p.CountryId,
                    p.Country?.Name ?? "Unknown",
                    p.Position,
                    p.PreferredFoot,
                    p.HeightCm,
                    p.WeightKg,
                    p.PhotoUrl,
                    p.IsActive,
                    teamName,
                    price,
                    0
                );
            })
            .ToList();

        return new PagedResult<PlayerDto, PlayerFilter>(items, totalCount, pageNumber, pageSize, filter);
    }
}
