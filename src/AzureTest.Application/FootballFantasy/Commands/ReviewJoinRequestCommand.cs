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

public sealed record ReviewJoinRequestCommand(Guid FantasyLeagueId, Guid TargetUserId, bool Approve) : ICommand<Result>;

public sealed class ReviewJoinRequestCommandHandler : BaseCommandHandler<ReviewJoinRequestCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public ReviewJoinRequestCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<ReviewJoinRequestCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(ReviewJoinRequestCommand cmd, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.FantasyLeagues.GetWithDetailsAsync(cmd.FantasyLeagueId, cancellationToken);
        if (league == null) return Result.Failure("Fantasy league not found.");

        if (league.OwnerUserId.HasValue && league.OwnerUserId.Value != _currentUser.Id && !_currentUser.IsAdmin)
        {
            return Result.Failure("Only the league creator or administrator can review join requests.");
        }

        var member = league.Members.FirstOrDefault(m => m.UserId == cmd.TargetUserId);
        var team = league.FantasyTeams.FirstOrDefault(t => t.UserId == cmd.TargetUserId);

        if (member == null && team == null)
        {
            return Result.Failure("No pending join request found for this user.");
        }

        if (cmd.Approve)
        {
            if (member != null) member.Approve();
            if (team != null) team.ApproveTeam();
        }
        else
        {
            if (member != null) member.Reject();
            if (team != null)
            {
                team.RejectTeam();
                team.IsDeleted = true;
                team.DeletedOn = DateTime.UtcNow;
                team.DeletedBy = _currentUser.Email ?? _currentUser.FullName;
            }
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
