using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetLeagueTournamentDetailsQuery(Guid FantasyLeagueId) : IQuery<Result<List<FantasyLeagueGroupDto>>>;

public sealed class GetLeagueTournamentDetailsQueryHandler : IQueryHandler<GetLeagueTournamentDetailsQuery, Result<List<FantasyLeagueGroupDto>>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetLeagueTournamentDetailsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<List<FantasyLeagueGroupDto>>> HandleAsync(GetLeagueTournamentDetailsQuery request, CancellationToken cancellationToken)
    {
        var league = await _uow.FantasyLeagues.GetWithDetailsAsync(request.FantasyLeagueId, cancellationToken);
        if (league == null) return Result<List<FantasyLeagueGroupDto>>.Failure("League not found.");

        var groups = await _uow.FantasyLeagueGroups.FindAsync(g => g.FantasyLeagueId == request.FantasyLeagueId);
        var matchups = await _uow.FantasyMatchups.FindAsync(m => m.FantasyLeagueId == request.FantasyLeagueId);
        var teamMap = league.FantasyTeams.ToDictionary(t => t.Id, t => t.Name);

        var result = new List<FantasyLeagueGroupDto>();

        if (groups.Count > 0)
        {
            foreach (var g in groups.OrderBy(x => x.GroupNumber))
            {
                var groupMatchups = matchups.Where(m => m.FantasyLeagueGroupId == g.Id).Select(m => new FantasyMatchupDto(
                    m.Id,
                    m.FantasyLeagueId,
                    m.FantasyLeagueGroupId,
                    g.Name,
                    m.PeriodId,
                    m.RoundNumber,
                    m.StageName,
                    m.MatchNumber,
                    m.HomeTeamId,
                    m.HomeTeamId.HasValue && teamMap.ContainsKey(m.HomeTeamId.Value) ? teamMap[m.HomeTeamId.Value] : "TBD",
                    m.HomeScore,
                    m.AwayTeamId,
                    m.AwayTeamId.HasValue && teamMap.ContainsKey(m.AwayTeamId.Value) ? teamMap[m.AwayTeamId.Value] : "TBD",
                    m.AwayScore,
                    m.WinnerTeamId,
                    m.WinnerTeamId.HasValue && teamMap.ContainsKey(m.WinnerTeamId.Value) ? teamMap[m.WinnerTeamId.Value] : null,
                    m.IsCompleted
                )).ToList();

                result.Add(new FantasyLeagueGroupDto(
                    g.Id,
                    g.FantasyLeagueId,
                    g.Name,
                    g.GroupNumber,
                    new List<FantasyTeamDto>(),
                    groupMatchups
                ));
            }
        }
        else
        {
            // Single list / bracket
            var allMatchups = matchups.OrderBy(m => m.RoundNumber).ThenBy(m => m.MatchNumber).Select(m => new FantasyMatchupDto(
                m.Id,
                m.FantasyLeagueId,
                null,
                null,
                m.PeriodId,
                m.RoundNumber,
                m.StageName,
                m.MatchNumber,
                m.HomeTeamId,
                m.HomeTeamId.HasValue && teamMap.ContainsKey(m.HomeTeamId.Value) ? teamMap[m.HomeTeamId.Value] : "TBD",
                m.HomeScore,
                m.AwayTeamId,
                m.AwayTeamId.HasValue && teamMap.ContainsKey(m.AwayTeamId.Value) ? teamMap[m.AwayTeamId.Value] : "TBD",
                m.AwayScore,
                m.WinnerTeamId,
                m.WinnerTeamId.HasValue && teamMap.ContainsKey(m.WinnerTeamId.Value) ? teamMap[m.WinnerTeamId.Value] : null,
                m.IsCompleted
            )).ToList();

            result.Add(new FantasyLeagueGroupDto(
                Guid.Empty,
                request.FantasyLeagueId,
                league.Format.ToString(),
                1,
                new List<FantasyTeamDto>(),
                allMatchups
            ));
        }

        return Result<List<FantasyLeagueGroupDto>>.Success(result);
    }
}
