using System;
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

public sealed record JoinPredictClubCommand(
    Guid PredictClubId,
    string? JoinCode = null
) : ICommand<Result>;

public sealed class JoinPredictClubCommandValidator : AbstractValidator<JoinPredictClubCommand>
{
    public JoinPredictClubCommandValidator()
    {
        RuleFor(x => x.PredictClubId).NotEmpty();
    }
}

public sealed class JoinPredictClubCommandHandler : BaseCommandHandler<JoinPredictClubCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public JoinPredictClubCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<JoinPredictClubCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(JoinPredictClubCommand cmd, CancellationToken cancellationToken)
    {
        var club = await UnitOfWork.PredictClubs.GetByIdAsync(cmd.PredictClubId);
        if (club == null) return Result.Failure("Predict Club not found.");

        var existingMembers = await UnitOfWork.PredictClubMembers.FindAsync(m => m.PredictClubId == cmd.PredictClubId && m.UserId == _currentUser.Id);
        var existing = existingMembers.Count > 0 ? existingMembers[0] : null;

        if (existing != null)
        {
            if (existing.Status == FantasyMemberStatus.Active)
            {
                return Result.Failure("You are already an active member of this Predict Club.");
            }
            if (existing.Status == FantasyMemberStatus.Left)
            {
                // Rejoin
                existing.UpdateStatus(FantasyMemberStatus.Active);
                await UnitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
        }

        // Check capacity
        if (club.MaxMembers.HasValue)
        {
            var activeMembers = await UnitOfWork.PredictClubMembers.FindAsync(m => m.PredictClubId == cmd.PredictClubId && m.Status == FantasyMemberStatus.Active);
            if (activeMembers.Count >= club.MaxMembers.Value)
            {
                return Result.Failure("This Predict Club has reached its maximum member capacity.");
            }
        }

        // Private club validation
        if (club.Visibility == FantasyLeagueVisibility.Private)
        {
            if (string.IsNullOrWhiteSpace(cmd.JoinCode) || !string.Equals(club.JoinCode, cmd.JoinCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure("Invalid private join code.");
            }
        }

        // Public leagues require owner approval; Global or private with valid code activate immediately
        var status = club.Visibility == FantasyLeagueVisibility.Public
            ? FantasyMemberStatus.PendingApproval
            : FantasyMemberStatus.Active;

        var member = PredictClubMember.Create(
            predictClubId: club.Id,
            userId: _currentUser.Id,
            role: FantasyMemberRole.Member,
            status: status
        );

        UnitOfWork.PredictClubMembers.Add(member);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
