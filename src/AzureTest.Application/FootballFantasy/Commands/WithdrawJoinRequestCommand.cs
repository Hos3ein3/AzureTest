using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record WithdrawJoinRequestCommand(Guid FantasyLeagueId) : ICommand<Result>;

public sealed class WithdrawJoinRequestCommandValidator : AbstractValidator<WithdrawJoinRequestCommand>
{
    public WithdrawJoinRequestCommandValidator()
    {
        RuleFor(x => x.FantasyLeagueId).NotEmpty();
    }
}

public sealed class WithdrawJoinRequestCommandHandler : BaseCommandHandler<WithdrawJoinRequestCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public WithdrawJoinRequestCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<WithdrawJoinRequestCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(WithdrawJoinRequestCommand cmd, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty) return Result.Failure("Unauthorized.");

        var members = await UnitOfWork.FantasyLeagueMembers.FindAsync(m => m.FantasyLeagueId == cmd.FantasyLeagueId && m.UserId == userId);
        var teams = await UnitOfWork.FantasyTeams.FindAsync(t => t.FantasyLeagueId == cmd.FantasyLeagueId && t.UserId == userId);

        var member = members.FirstOrDefault();
        var team = teams.FirstOrDefault();

        if (member == null && team == null)
        {
            return Result.Failure("No join request found for this fantasy league.");
        }

        // Only allow withdrawing if still pending approval
        if (member != null && member.Status != FantasyMemberStatus.PendingApproval && team != null && team.IsApproved)
        {
            return Result.Failure("You are already an approved member of this league. Use Leave League instead.");
        }

        if (member != null)
        {
            member.Remove();
            member.IsDeleted = true;
            member.DeletedOn = DateTime.UtcNow;
            member.DeletedBy = _currentUser.Email ?? _currentUser.FullName;
        }

        if (team != null)
        {
            team.IsDeleted = true;
            team.DeletedOn = DateTime.UtcNow;
            team.DeletedBy = _currentUser.Email ?? _currentUser.FullName;
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
