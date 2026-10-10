using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record InitializeLeagueTournamentCommand(Guid FantasyLeagueId) : ICommand<Result>;

public sealed class InitializeLeagueTournamentCommandValidator : AbstractValidator<InitializeLeagueTournamentCommand>
{
    public InitializeLeagueTournamentCommandValidator()
    {
        RuleFor(x => x.FantasyLeagueId).NotEmpty();
    }
}

public sealed class InitializeLeagueTournamentCommandHandler : BaseCommandHandler<InitializeLeagueTournamentCommand, Result>
{
    public InitializeLeagueTournamentCommandHandler(IUnitOfWork uow, ILogger<InitializeLeagueTournamentCommandHandler> logger)
        : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(InitializeLeagueTournamentCommand cmd, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.FantasyLeagues.GetWithDetailsAsync(cmd.FantasyLeagueId, cancellationToken);
        if (league == null) return Result.Failure("Fantasy league not found.");

        var teams = league.FantasyTeams.Where(t => t.IsApproved && !t.IsSpectator).ToList();
        if (teams.Count < 2) return Result.Failure("At least 2 active teams are required to initialize tournament/matchups.");

        // Clear existing matchups and groups if any
        var existingMatchups = await UnitOfWork.FantasyMatchups.FindAsync(m => m.FantasyLeagueId == cmd.FantasyLeagueId);
        foreach (var m in existingMatchups) UnitOfWork.FantasyMatchups.Remove(m);

        var existingGroups = await UnitOfWork.FantasyLeagueGroups.FindAsync(g => g.FantasyLeagueId == cmd.FantasyLeagueId);
        foreach (var g in existingGroups) UnitOfWork.FantasyLeagueGroups.Remove(g);

        if (league.Format == FantasyLeagueFormat.HeadToHead)
        {
            // Generate Round-Robin Head-to-Head schedule
            GenerateHeadToHeadSchedule(league.Id, teams);
        }
        else if (league.Format == FantasyLeagueFormat.Knockout)
        {
            // Generate Single Elimination Bracket
            GenerateKnockoutBracket(league.Id, teams);
        }
        else if (league.Format == FantasyLeagueFormat.GroupedKnockout)
        {
            // Randomly divide into N groups, then setup group matchups
            var numGroups = league.GroupCount.HasValue && league.GroupCount.Value > 1 ? league.GroupCount.Value : 2;
            GenerateGroupedKnockout(league.Id, teams, numGroups);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private void GenerateHeadToHeadSchedule(Guid leagueId, List<FantasyTeam> teams)
    {
        var teamList = new List<FantasyTeam>(teams);
        // If odd, add a dummy bye
        bool hasBye = teamList.Count % 2 != 0;
        int totalRounds = hasBye ? teamList.Count : teamList.Count - 1;
        int matchesPerRound = (teamList.Count + (hasBye ? 1 : 0)) / 2;

        int matchCounter = 1;
        for (int round = 1; round <= totalRounds; round++)
        {
            for (int i = 0; i < matchesPerRound; i++)
            {
                int homeIdx = (round + i) % teamList.Count;
                int awayIdx = (round + teamList.Count - i) % teamList.Count;

                if (i == 0 && !hasBye) awayIdx = teamList.Count - 1;

                if (homeIdx == awayIdx) continue;

                var home = teamList[homeIdx];
                var away = teamList[awayIdx];

                var matchup = FantasyMatchup.Create(
                    leagueId,
                    round,
                    $"Gameweek {round}",
                    matchCounter++,
                    home.Id,
                    away.Id
                );
                UnitOfWork.FantasyMatchups.Add(matchup);
            }
        }
    }

    private void GenerateKnockoutBracket(Guid leagueId, List<FantasyTeam> teams)
    {
        // Shuffle teams randomly
        var shuffled = teams.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToList();
        int matchCount = shuffled.Count / 2;

        for (int i = 0; i < matchCount; i++)
        {
            var home = shuffled[i * 2];
            var away = (i * 2 + 1 < shuffled.Count) ? shuffled[i * 2 + 1] : null;

            var matchup = FantasyMatchup.Create(
                leagueId,
                1,
                "Round of " + (matchCount * 2),
                i + 1,
                home.Id,
                away?.Id
            );
            UnitOfWork.FantasyMatchups.Add(matchup);
        }
    }

    private void GenerateGroupedKnockout(Guid leagueId, List<FantasyTeam> teams, int groupCount)
    {
        // Shuffle teams
        var shuffled = teams.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToList();
        var groups = new List<FantasyLeagueGroup>();

        for (int g = 0; g < groupCount; g++)
        {
            char letter = (char)('A' + g);
            var group = FantasyLeagueGroup.Create(leagueId, $"Group {letter}", g + 1);
            groups.Add(group);
            UnitOfWork.FantasyLeagueGroups.Add(group);
        }

        // Distribute teams evenly into groups
        for (int i = 0; i < shuffled.Count; i++)
        {
            var group = groups[i % groupCount];
            // Generate matches within group as round robin
        }

        // For each group, create group matches
        int matchCounter = 1;
        for (int g = 0; g < groupCount; g++)
        {
            var group = groups[g];
            var groupTeams = shuffled.Where((_, idx) => idx % groupCount == g).ToList();

            for (int i = 0; i < groupTeams.Count; i++)
            {
                for (int j = i + 1; j < groupTeams.Count; j++)
                {
                    var matchup = FantasyMatchup.Create(
                        leagueId,
                        1,
                        $"{group.Name} Match",
                        matchCounter++,
                        groupTeams[i].Id,
                        groupTeams[j].Id,
                        group.Id
                    );
                    UnitOfWork.FantasyMatchups.Add(matchup);
                }
            }
        }
    }
}
