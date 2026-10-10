using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record ProcessLeagueKickoutsCommand(Guid FantasyLeagueId) : ICommand<Result<int>>;

public sealed class ProcessLeagueKickoutsCommandHandler : BaseCommandHandler<ProcessLeagueKickoutsCommand, Result<int>>
{
    private readonly ICurrentUser _currentUser;

    public ProcessLeagueKickoutsCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<ProcessLeagueKickoutsCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<int>> ExecuteAsync(ProcessLeagueKickoutsCommand cmd, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.FantasyLeagues.GetWithDetailsAsync(cmd.FantasyLeagueId, cancellationToken);
        if (league == null) return Result<int>.Failure("Fantasy league not found.");

        if (league.OwnerUserId.HasValue && league.OwnerUserId.Value != _currentUser.Id && !_currentUser.IsAdmin)
        {
            return Result<int>.Failure("Only league owner or administrator can process unfinalized kickouts.");
        }

        var unfinalizedTeams = league.FantasyTeams.Where(t => !t.IsFinalized).ToList();
        int affectedCount = 0;

        foreach (var team in unfinalizedTeams)
        {
            var member = league.Members.FirstOrDefault(m => m.UserId == team.UserId);

            if (league.KickoutType == FantasyKickoutType.FullRemoval)
            {
                // Hard kickout: completely remove member and team
                team.IsDeleted = true;
                team.DeletedOn = DateTime.UtcNow;
                team.DeletedBy = _currentUser.Email ?? _currentUser.FullName;

                if (member != null)
                {
                    member.Remove();
                }
            }
            else // SpectatorOnly
            {
                // Soft kickout: disqualify team, keep user as read-only spectator
                team.MakeSpectator();
                if (member != null)
                {
                    member.MakeSpectator();
                }
            }

            affectedCount++;
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(affectedCount);
    }
}
