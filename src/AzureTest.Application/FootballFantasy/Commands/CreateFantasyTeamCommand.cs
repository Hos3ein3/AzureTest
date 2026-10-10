using System;
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

public sealed record CreateFantasyTeamCommand(
    Guid FantasyLeagueId,
    string Name,
    string? LogoUrl,
    string? JoinCode = null,
    Guid? CoachId = null
) : ICommand<Result<Guid>>;

public sealed class CreateFantasyTeamCommandHandler : BaseCommandHandler<CreateFantasyTeamCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;

    public CreateFantasyTeamCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<CreateFantasyTeamCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateFantasyTeamCommand cmd, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty) return Result<Guid>.Failure("Unauthorized.");

        var league = await UnitOfWork.FantasyLeagues.GetWithDetailsAsync(cmd.FantasyLeagueId, cancellationToken);
        if (league == null) return Result<Guid>.Failure("League not found.");

        if (league.Visibility == FantasyLeagueVisibility.Private)
        {
            if (!string.IsNullOrWhiteSpace(league.JoinCode) && !string.Equals(league.JoinCode, cmd.JoinCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result<Guid>.Failure("Invalid join code for this private league.");
            }
        }

        var existingTeams = await UnitOfWork.FantasyTeams.FindAsync(t => t.FantasyLeagueId == cmd.FantasyLeagueId && t.UserId == userId);
        if (existingTeams.Count > 0) return Result<Guid>.Failure("You already have a fantasy team in this league.");

        if (league.MaxMembers.HasValue && league.FantasyTeams.Count >= league.MaxMembers.Value)
            return Result<Guid>.Failure("This fantasy league is full.");

        Guid? coachId = null;
        if (league.AllowCoach && cmd.CoachId.HasValue && cmd.CoachId.Value != Guid.Empty)
        {
            var coach = await UnitOfWork.Coaches.GetByIdAsync(cmd.CoachId.Value);
            if (coach != null) coachId = coach.Id;
        }

        bool requiresApproval = league.Visibility == FantasyLeagueVisibility.Public 
                                && league.OwnerUserId.HasValue 
                                && league.OwnerUserId.Value != userId 
                                && !_currentUser.IsAdmin;

        var team = FantasyTeam.Create(league.Id, userId, cmd.Name, league.Budget, cmd.LogoUrl, coachId);
        if (requiresApproval)
        {
            team.RejectTeam(); // IsApproved = false pending owner approval
        }
        UnitOfWork.FantasyTeams.Add(team);

        var existingMembers = await UnitOfWork.FantasyLeagueMembers.FindAsync(m => m.FantasyLeagueId == league.Id && m.UserId == userId);
        if (existingMembers.Count == 0)
        {
            var initialStatus = requiresApproval ? FantasyMemberStatus.PendingApproval : FantasyMemberStatus.Active;
            var member = FantasyLeagueMember.Create(league.Id, userId, FantasyMemberRole.Member, initialStatus);
            UnitOfWork.FantasyLeagueMembers.Add(member);
        }

        return Result<Guid>.Success(team.Id);
    }
}
