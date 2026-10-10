using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetFantasyAdminDashboardQuery() : IQuery<Result<FantasyAdminDashboardDto>>;

public record FantasyAdminDashboardDto(
    int CountriesCount,
    int LeaguesCount,
    int SeasonsCount,
    int TeamsCount,
    int PlayersCount,
    int FixturesCount,
    int ScoringRulesCount,
    int FantasyLeaguesCount,
    List<CountryDto> RecentCountries,
    List<LeagueDto> RecentLeagues,
    List<TeamDto> RecentTeams,
    List<PlayerDto> RecentPlayers,
    List<FixtureDto> RecentFixtures
);

public sealed class GetFantasyAdminDashboardQueryHandler : BaseQueryHandler<GetFantasyAdminDashboardQuery, Result<FantasyAdminDashboardDto>>
{
    private readonly IUnitOfWork _uow;

    public GetFantasyAdminDashboardQueryHandler(IUnitOfWork uow, ILogger<GetFantasyAdminDashboardQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<FantasyAdminDashboardDto>> ExecuteAsync(GetFantasyAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var countries = await _uow.Countries.GetAllAsync();
        var leagues = await _uow.Leagues.GetAllAsync();
        var seasons = await _uow.Seasons.GetAllAsync();
        var teams = await _uow.Teams.GetAllAsync();
        var players = await _uow.Players.GetPlayersWithDetailsAsync(cancellationToken);
        var fixtures = await _uow.Fixtures.GetAllAsync();
        var rules = await _uow.ScoringRuleSets.GetAllAsync();
        var fLeagues = await _uow.FantasyLeagues.GetAllAsync();

        var recentCountries = countries.Take(10).Select(c => new CountryDto(c.Id, c.Name, c.Code, c.Continent, c.FlagUrl)).ToList();
        var recentLeagues = leagues.Take(10).Select(l => new LeagueDto(l.Id, l.Name, l.ShortName, l.Type, l.Level, null, l.Continent, l.LogoUrl, l.IsActive)).ToList();
        var recentTeams = teams.Take(10).Select(t => new TeamDto(t.Id, t.Name, t.ShortName, t.Code, t.TeamType, t.CountryId, "", t.FoundedYear, t.StadiumName, t.StadiumCapacity, t.LogoUrl)).ToList();
        var recentPlayers = players.Take(10).Select(p => new PlayerDto(
            p.Id, p.FirstName, p.LastName, p.DisplayName, p.DateOfBirth, p.CountryId, p.Country?.Name ?? "", p.Position, p.PreferredFoot, p.HeightCm, p.WeightKg, p.PhotoUrl, p.IsActive,
            p.PlayerSquads.Where(ps => ps.ToDate == null).Select(ps => ps.TeamSeason?.Team?.ShortName).FirstOrDefault() ?? "Free Agent",
            p.Prices.OrderByDescending(pr => pr.ValidFrom).Select(pr => pr.Price).FirstOrDefault() == 0 ? 5.0m : p.Prices.OrderByDescending(pr => pr.ValidFrom).Select(pr => pr.Price).FirstOrDefault(),
            0
        )).ToList();
        var recentFixtures = fixtures.Take(10).Select(f => new FixtureDto(f.Id, f.SeasonId, "Home", null, "Away", null, f.KickoffAt, f.Status, f.HomeScore, f.AwayScore, f.Minute, f.Round, f.Venue, f.IsStatsFinalized)).ToList();

        var dto = new FantasyAdminDashboardDto(
            countries.Count,
            leagues.Count,
            seasons.Count,
            teams.Count,
            players.Count,
            fixtures.Count,
            rules.Count,
            fLeagues.Count,
            recentCountries,
            recentLeagues,
            recentTeams,
            recentPlayers,
            recentFixtures
        );

        return Result<FantasyAdminDashboardDto>.Success(dto);
    }
}
