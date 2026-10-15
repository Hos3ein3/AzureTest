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
            var existingSquadEntries = team.SquadEntries.Where(se => !se.IsDeleted).ToList();
            var existingSquadPlayerIds = existingSquadEntries.Select(se => se.PlayerId).ToHashSet();
            var newPlayerIds = requestedPlayerIds.Where(pid => !existingSquadPlayerIds.Contains(pid)).ToList();
            var removedPlayerIds = existingSquadPlayerIds.Where(pid => !requestedPlayerIds.Contains(pid)).ToList();
            bool isTransferring = newPlayerIds.Count > 0 || removedPlayerIds.Count > 0;

            // Roster lock enforcement: If team already has a squad and is transferring players
            if (existingSquadPlayerIds.Count > 0 && isTransferring && !league.IsTransferMarketOpen())
            {
                return Result.Failure("Transfer market is currently closed. You cannot buy or sell players outside the official transfer windows, but you can rearrange your starting lineup and bench.");
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

            var allPlayers = await UnitOfWork.Players.GetPlayersWithDetailsAsync(cancellationToken);
            var playerMap = allPlayers.ToDictionary(p => p.Id);

            decimal GetPlayerPrice(Player player)
            {
                var latest = player.Prices.OrderByDescending(pr => pr.ValidFrom).Select(pr => pr.Price).FirstOrDefault();
                return latest == 0 ? 5.0m : latest;
            }

            // Formation Position Validation: Ensure starters match formation position counts (e.g. 4-3-3 requires 1 GK, 4 DEF, 3 MID, 3 FWD)
            var starterSlots = req.Slots.Where(s => s.IsStarter).ToList();
            if (starterSlots.Count == league.StartingSize && !string.IsNullOrWhiteSpace(req.Formation))
            {
                var formParts = req.Formation.Split(new[] { '-', 'x', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (formParts.Length == 3 &&
                    int.TryParse(formParts[0], out int reqDef) &&
                    int.TryParse(formParts[1], out int reqMid) &&
                    int.TryParse(formParts[2], out int reqFwd))
                {
                    var starterPlayerEntities = allPlayers.Where(p => starterSlots.Any(ss => ss.PlayerId == p.Id)).ToList();
                    int actualGk = starterPlayerEntities.Count(p => p.Position == PlayerPosition.Goalkeeper);
                    int actualDef = starterPlayerEntities.Count(p => p.Position == PlayerPosition.Defender);
                    int actualMid = starterPlayerEntities.Count(p => p.Position == PlayerPosition.Midfielder);
                    int actualFwd = starterPlayerEntities.Count(p => p.Position == PlayerPosition.Forward);

                    if (actualGk != 1)
                    {
                        return Result.Failure($"A starting lineup must have exactly 1 Goalkeeper (found {actualGk}).");
                    }
                    if (actualDef != reqDef)
                    {
                        return Result.Failure($"Formation '{req.Formation}' requires exactly {reqDef} Defenders, but {actualDef} were selected in the starting lineup.");
                    }
                    if (actualMid != reqMid)
                    {
                        return Result.Failure($"Formation '{req.Formation}' requires exactly {reqMid} Midfielders, but {actualMid} were selected in the starting lineup.");
                    }
                    if (actualFwd != reqFwd)
                    {
                        return Result.Failure($"Formation '{req.Formation}' requires exactly {reqFwd} Forwards/Attackers, but {actualFwd} were selected in the starting lineup.");
                    }
                }
            }

            // Enforce transfer rules: 1 free transfer per gameweek, extra transfers point penalty
            if (existingSquadPlayerIds.Count > 0 && newPlayerIds.Count > 0)
            {
                int newTransfersCount = newPlayerIds.Count;
                int currentTransfersMade = team.GameweekTransfersMade;
                int freeTransfersAllowed = Math.Max(0, league.FreeTransfersPerGameweek);

                for (int i = 0; i < newTransfersCount; i++)
                {
                    int transferIndex = currentTransfersMade + i + 1;
                    if (transferIndex > freeTransfersAllowed)
                    {
                        if (!league.ExtraTransferPointPenalty.HasValue)
                        {
                            return Result.Failure($"Extra transfers beyond the {freeTransfersAllowed} free transfer(s) per gameweek are not permitted in this league.");
                        }

                        int penalty = league.ExtraTransferPointPenalty.Value;
                        team.RecordTransfer(penalty);
                    }
                    else
                    {
                        team.RecordTransfer(0);
                    }
                }
            }

            // Auction Mode Rule: Player & Coach Uniqueness across the entire league
            if (league.Mode == FantasyLeagueMode.Auction)
            {
                var otherLeagueTeams = await UnitOfWork.FantasyTeams.FindAsync(t => t.FantasyLeagueId == league.Id && t.Id != team.Id);
                var takenPlayerIds = otherLeagueTeams
                    .SelectMany(t => t.SquadEntries)
                    .Where(se => !se.IsDeleted)
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

            // Handle player sales (credit refunds with profit tax deduction)
            foreach (var remId in removedPlayerIds)
            {
                var entry = existingSquadEntries.FirstOrDefault(e => e.PlayerId == remId);
                if (entry != null)
                {
                    decimal purchasePrice = entry.PricePaid;
                    decimal currentPrice = playerMap.TryGetValue(remId, out var p) ? GetPlayerPrice(p) : purchasePrice;
                    decimal sellPrice = league.CalculateSellPrice(purchasePrice, currentPrice);
                    team.AddCredits(sellPrice);

                    entry.IsDeleted = true;
                    entry.DeletedOn = DateTime.UtcNow;
                    entry.DeletedBy = _currentUser.Email ?? _currentUser.FullName;
                }
            }

            // Handle player purchases
            foreach (var addId in newPlayerIds)
            {
                if (playerMap.TryGetValue(addId, out var playerToBuy))
                {
                    decimal price = GetPlayerPrice(playerToBuy);
                    if (team.CreditsLeft < price)
                    {
                        return Result.Failure($"Insufficient credits to buy {playerToBuy.DisplayName} (Costs £{price:F1}m, available £{team.CreditsLeft:F1}m).");
                    }

                    team.DeductCredits(price);
                    var newEntry = FantasySquadEntry.Create(team.Id, playerToBuy.Id, price);
                    UnitOfWork.FantasySquadEntries.Add(newEntry);
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

        // Save Lineup Slots
        if (req.Slots != null && req.Slots.Count > 0)
        {
            var existingSlots = await UnitOfWork.FantasyLineupSlots.FindAsync(s => s.FantasyLineupId == lineup.Id);
            foreach (var s in existingSlots)
            {
                s.IsDeleted = true;
                s.DeletedOn = DateTime.UtcNow;
            }

            foreach (var slotReq in req.Slots)
            {
                var slot = FantasyLineupSlot.Create(lineup.Id, slotReq.PlayerId, slotReq.IsStarter, slotReq.BenchOrder);
                UnitOfWork.FantasyLineupSlots.Add(slot);
            }
        }

        // Finalize squad status if manager has met the league's MinSquadSize rule
        if (req.Slots != null && req.Slots.Count >= league.MinSquadSize)
        {
            team.FinalizeSquad();
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
