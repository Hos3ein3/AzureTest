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

public sealed record SaveTournamentPredictionCommand(
    Guid PredictClubId,
    Guid? FirstPlaceTeamId = null,
    Guid? SecondPlaceTeamId = null,
    Guid? ThirdPlaceTeamId = null,
    Guid? BestPlayerId = null,
    Guid? TopScorerPlayerId = null,
    Guid? TopAssisterPlayerId = null,
    bool Submit = false
) : ICommand<Result>;

public sealed class SaveTournamentPredictionCommandValidator : AbstractValidator<SaveTournamentPredictionCommand>
{
    public SaveTournamentPredictionCommandValidator()
    {
        RuleFor(x => x.PredictClubId).NotEmpty();
    }
}

public sealed class SaveTournamentPredictionCommandHandler : BaseCommandHandler<SaveTournamentPredictionCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public SaveTournamentPredictionCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<SaveTournamentPredictionCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(SaveTournamentPredictionCommand cmd, CancellationToken cancellationToken)
    {
        var club = await UnitOfWork.PredictClubs.GetByIdAsync(cmd.PredictClubId);
        if (club == null) return Result.Failure("Predict Club not found.");

        if (DateTime.UtcNow >= club.StartsAt)
        {
            return Result.Failure("Tournament level predictions are locked after the competition begins.");
        }

        var memberList = await UnitOfWork.PredictClubMembers.FindAsync(m => m.PredictClubId == cmd.PredictClubId && m.UserId == _currentUser.Id && m.Status == FantasyMemberStatus.Active);
        if (memberList.Count == 0)
        {
            return Result.Failure("You must be an active member of this Predict Club.");
        }

        var existingList = await UnitOfWork.TournamentPredictions.FindAsync(p => p.PredictClubId == cmd.PredictClubId && p.UserId == _currentUser.Id);
        var prediction = existingList.Count > 0 ? existingList[0] : null;

        if (prediction == null)
        {
            prediction = TournamentPrediction.Create(
                predictClubId: cmd.PredictClubId,
                userId: _currentUser.Id,
                firstPlaceTeamId: cmd.FirstPlaceTeamId,
                secondPlaceTeamId: cmd.SecondPlaceTeamId,
                thirdPlaceTeamId: cmd.ThirdPlaceTeamId,
                bestPlayerId: cmd.BestPlayerId,
                topScorerPlayerId: cmd.TopScorerPlayerId,
                topAssisterPlayerId: cmd.TopAssisterPlayerId,
                isDraft: !cmd.Submit
            );
            UnitOfWork.TournamentPredictions.Add(prediction);
        }
        else
        {
            prediction.UpdateDraft(
                firstPlaceTeamId: cmd.FirstPlaceTeamId,
                secondPlaceTeamId: cmd.SecondPlaceTeamId,
                thirdPlaceTeamId: cmd.ThirdPlaceTeamId,
                bestPlayerId: cmd.BestPlayerId,
                topScorerPlayerId: cmd.TopScorerPlayerId,
                topAssisterPlayerId: cmd.TopAssisterPlayerId,
                submit: cmd.Submit
            );
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
