using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record SaveLineupCommand(SaveLineupRequest Request) : ICommand<Result>;

public sealed class SaveLineupCommandHandler : BaseCommandHandler<SaveLineupCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public SaveLineupCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<SaveLineupCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(SaveLineupCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var team = await UnitOfWork.FantasyTeams.GetByIdWithDetailsAsync(req.FantasyTeamId, cancellationToken);
        if (team == null) return Result.Failure("Team not found.");

        if (team.UserId != _currentUser.Id && !_currentUser.IsAdmin)
            return Result.Failure("Unauthorized.");

        var league = await UnitOfWork.FantasyLeagues.GetWithDetailsAsync(team.FantasyLeagueId, cancellationToken);
        if (league == null) return Result.Failure("Fantasy league not found.");

        // Rule validation 1: Check allowed formations
        if (!string.IsNullOrWhiteSpace(league.AllowedFormations))
        {
            var allowed = league.AllowedFormations
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .ToList();

            if (allowed.Count > 0 && !allowed.Contains(req.Formation, StringComparer.OrdinalIgnoreCase))
            {
                return Result.Failure($"Formation '{req.Formation}' is not permitted in this league. Allowed: {league.AllowedFormations}");
            }
        }

        // Rule validation 2: Validate player budget and team limits if slots are specified
        if (req.Slots != null && req.Slots.Count > 0)
        {
            var requestedPlayerIds = req.Slots.Select(s => s.PlayerId).Distinct().ToList();
            var existingSquadPlayerIds = team.SquadEntries.Select(se => se.PlayerId).ToHashSet();
            bool isAddingNewPlayers = requestedPlayerIds.Any(pid => !existingSquadPlayerIds.Contains(pid));

            // If team already has a registered squad and is trying to buy/sell outside market window
            if (existingSquadPlayerIds.Count > 0 && isAddingNewPlayers && !league.IsTransferMarketOpen())
            {
                return Result.Failure("Transfer market is currently closed. You cannot buy new players outside the official transfer windows, but you can rearrange your lineup and bench.");
            }

            if (req.Slots.Count > league.SquadSize)
            {
                return Result.Failure($"Total squad players ({req.Slots.Count}) cannot exceed league squad limit ({league.SquadSize}).");
            }

            var starterCount = req.Slots.Count(s => s.IsStarter);
            if (starterCount > league.StartingSize)
            {
                return Result.Failure($"Starting players ({starterCount}) cannot exceed league starting limit ({league.StartingSize}).");
            }

            // Auction Mode Rule: Player & Coach Uniqueness across the entire league
            if (league.Mode == FantasyLeagueMode.Auction)
            {
                var otherLeagueTeams = await UnitOfWork.FantasyTeams.FindAsync(t => t.FantasyLeagueId == league.Id && t.Id != team.Id);
                var takenPlayerIds = otherLeagueTeams
                    .SelectMany(t => t.SquadEntries)
                    .Select(se => se.PlayerId)
                    .ToHashSet();

                var duplicateInLeague = requestedPlayerIds.FirstOrDefault(pid => takenPlayerIds.Contains(pid));
                if (duplicateInLeague != Guid.Empty)
                {
                    return Result.Failure("In Auction leagues, each player can only belong to one fantasy team. A selected player is already owned by another team.");
                }

                if (req.CoachId.HasValue && req.CoachId.Value != Guid.Empty)
                {
                    bool coachTaken = otherLeagueTeams.Any(t => t.CoachId == req.CoachId.Value);
                    if (coachTaken)
                    {
                        return Result.Failure("In Auction leagues, each Head Coach can only manage one fantasy team. This coach is already recruited by another team.");
                    }
                }
            }

            // Check max players per real-world team if configured
            if (league.MaxPlayersPerRealTeam.HasValue && league.MaxPlayersPerRealTeam.Value > 0)
            {
                var allPlayers = await UnitOfWork.Players.GetPlayersWithDetailsAsync(cancellationToken);
                var squadPlayers = allPlayers.Where(p => requestedPlayerIds.Contains(p.Id)).ToList();

                var teamsGrouped = squadPlayers
                    .Select(p => p.PlayerSquads.Where(ps => ps.ToDate == null).Select(ps => ps.TeamSeason?.TeamId).FirstOrDefault())
                    .Where(tid => tid.HasValue)
                    .GroupBy(tid => tid!.Value)
                    .ToList();

                foreach (var g in teamsGrouped)
                {
                    if (g.Count() > league.MaxPlayersPerRealTeam.Value)
                    {
                        return Result.Failure($"Squad exceeds maximum allowed players from the same real team (Limit: {league.MaxPlayersPerRealTeam.Value}).");
                    }
                }
            }
        }

        if (league.AllowCoach && req.CoachId.HasValue)
        {
            if (req.CoachId.Value == Guid.Empty)
            {
                team.SetCoach(null);
            }
            else
            {
                var coach = await UnitOfWork.Coaches.GetByIdAsync(req.CoachId.Value);
                if (coach != null)
                {
                    team.SetCoach(coach.Id);
                }
            }
        }

        Guid periodId = req.FantasyPeriodId;
        if (periodId == Guid.Empty)
        {
            var periods = await UnitOfWork.FantasyPeriods.FindAsync(p => p.FantasyLeagueId == league.Id);
            var activePeriod = periods.FirstOrDefault(p => p.Status == FantasyPeriodStatus.Open) 
                               ?? periods.FirstOrDefault(p => p.Status == FantasyPeriodStatus.Upcoming)
                               ?? periods.FirstOrDefault();

            if (activePeriod != null)
            {
                periodId = activePeriod.Id;
            }
            else
            {
                var now = DateTime.UtcNow;
                var newPeriod = FantasyPeriod.Create(league.Id, 1, "Gameweek 1", now, now.AddDays(7), now.AddHours(-1));
                UnitOfWork.FantasyPeriods.Add(newPeriod);
                periodId = newPeriod.Id;
            }
        }

        var existingLineups = await UnitOfWork.FantasyLineups.FindAsync(l => l.FantasyTeamId == req.FantasyTeamId && l.FantasyPeriodId == periodId);
        FantasyLineup lineup;

        if (existingLineups.Count > 0)
        {
            lineup = existingLineups[0];
            if (lineup.IsLocked) return Result.Failure("Lineup is locked for this gameweek period.");
            lineup.UpdateFormation(req.Formation, req.CaptainPlayerId, req.ViceCaptainPlayerId);
        }
        else
        {
            lineup = FantasyLineup.Create(req.FantasyTeamId, periodId, req.Formation, req.CaptainPlayerId, req.ViceCaptainPlayerId);
            UnitOfWork.FantasyLineups.Add(lineup);
        }

        // Finalize squad status if manager has met the league's MinSquadSize rule
        if (req.Slots != null && req.Slots.Count >= league.MinSquadSize)
        {
            team.FinalizeSquad();
        }

        return Result.Success();
    }
}
