using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record LeaveFantasyLeagueCommand(Guid FantasyLeagueId) : ICommand<Result>;

public sealed class LeaveFantasyLeagueCommandValidator : AbstractValidator<LeaveFantasyLeagueCommand>
{
    public LeaveFantasyLeagueCommandValidator()
    {
        RuleFor(x => x.FantasyLeagueId).NotEmpty();
    }
}

public sealed class LeaveFantasyLeagueCommandHandler : BaseCommandHandler<LeaveFantasyLeagueCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public LeaveFantasyLeagueCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<LeaveFantasyLeagueCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(LeaveFantasyLeagueCommand cmd, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.FantasyLeagues.GetByIdAsync(cmd.FantasyLeagueId);
        if (league == null) return Result.Failure("Fantasy league not found.");

        if (league.OwnerUserId == _currentUser.Id)
        {
            return Result.Failure("League owner cannot leave their own league. Transfer ownership or archive the league instead.");
        }

        var members = await UnitOfWork.FantasyLeagueMembers.FindAsync(m => m.FantasyLeagueId == cmd.FantasyLeagueId && m.UserId == _currentUser.Id);
        if (members.Count == 0) return Result.Failure("You are not a member of this fantasy league.");

        var member = members[0];
        // Mark member as Left so it remains permanently tracked in historical audit/profile
        member.Leave();

        // Demote their team or remove from active standings
        var teams = await UnitOfWork.FantasyTeams.FindAsync(t => t.FantasyLeagueId == cmd.FantasyLeagueId && t.UserId == _currentUser.Id);
        if (teams.Count > 0)
        {
            teams[0].MakeSpectator();
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
