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

public sealed record GetAdminFantasyTeamsQuery() : IQuery<Result<List<AdminFantasyTeamOverviewDto>>>;

public sealed class GetAdminFantasyTeamsQueryHandler : BaseQueryHandler<GetAdminFantasyTeamsQuery, Result<List<AdminFantasyTeamOverviewDto>>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetAdminFantasyTeamsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetAdminFantasyTeamsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<List<AdminFantasyTeamOverviewDto>>> ExecuteAsync(GetAdminFantasyTeamsQuery request, CancellationToken cancellationToken)
    {
        var fantasyTeams = await _uow.FantasyTeams.GetAllAsync();
        var fantasyLeagues = await _uow.FantasyLeagues.GetAllAsync();
        var leagueMap = fantasyLeagues.ToDictionary(l => l.Id, l => l);

        var list = new List<AdminFantasyTeamOverviewDto>();

        foreach (var ft in fantasyTeams)
        {
            var lName = leagueMap.TryGetValue(ft.FantasyLeagueId, out var fl) ? fl.Name : "Unknown";
            var lTag = fl != null ? fl.LeagueTag : "N/A";
            var squad = await _uow.FantasySquadEntries.FindAsync(se => se.FantasyTeamId == ft.Id);

            list.Add(new AdminFantasyTeamOverviewDto(
                ft.Id,
                ft.Name,
                ft.LogoUrl,
                ft.FantasyLeagueId,
                lName,
                lTag,
                ft.UserId,
                $"User {ft.UserId.ToString().Substring(0, 8)}",
                ft.TotalPoints,
                ft.CreditsLeft,
                squad.Count,
                ft.CreatedOn
            ));
        }

        return Result<List<AdminFantasyTeamOverviewDto>>.Success(list.OrderByDescending(x => x.TotalPoints).ToList());
    }
}
