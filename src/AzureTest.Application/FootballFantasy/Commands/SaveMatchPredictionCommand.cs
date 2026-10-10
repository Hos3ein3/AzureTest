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

public sealed record SaveMatchPredictionCommand(
    Guid PredictClubId,
    Guid FixtureId,
    int? HomeScore = null,
    int? AwayScore = null,
    int? HalfTimeHomeScore = null,
    int? HalfTimeAwayScore = null,
    bool? BothTeamsToScore = null,
    decimal? OverUnder = null,
    int? YellowCards = null,
    int? Corners = null,
    int? FirstGoalMinute = null,
    Guid? MvpPlayerId = null,
    bool Submit = false
) : ICommand<Result>;

public sealed class SaveMatchPredictionCommandValidator : AbstractValidator<SaveMatchPredictionCommand>
{
    public SaveMatchPredictionCommandValidator()
    {
        RuleFor(x => x.PredictClubId).NotEmpty();
        RuleFor(x => x.FixtureId).NotEmpty();
    }
}

public sealed class SaveMatchPredictionCommandHandler : BaseCommandHandler<SaveMatchPredictionCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public SaveMatchPredictionCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<SaveMatchPredictionCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(SaveMatchPredictionCommand cmd, CancellationToken cancellationToken)
    {
        var club = await UnitOfWork.PredictClubs.GetByIdAsync(cmd.PredictClubId);
        if (club == null) return Result.Failure("Predict Club not found.");

        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(cmd.FixtureId);
        if (fixture == null) return Result.Failure("Fixture not found.");

        // Rule enforcement: Predictions finalize 1 minute before kickoff
        if (DateTime.UtcNow >= fixture.KickoffAt.AddMinutes(-1))
        {
            return Result.Failure("Predictions for this fixture are closed. Selections lock 1 minute before kickoff.");
        }

        var memberList = await UnitOfWork.PredictClubMembers.FindAsync(m => m.PredictClubId == cmd.PredictClubId && m.UserId == _currentUser.Id && m.Status == FantasyMemberStatus.Active);
        if (memberList.Count == 0)
        {
            return Result.Failure("You must be an active member of this Predict Club to submit predictions.");
        }

        var existingList = await UnitOfWork.MatchPredictions.FindAsync(p => p.PredictClubId == cmd.PredictClubId && p.FixtureId == cmd.FixtureId && p.UserId == _currentUser.Id);
        var prediction = existingList.Count > 0 ? existingList[0] : null;

        if (prediction == null)
        {
            prediction = MatchPrediction.Create(
                predictClubId: cmd.PredictClubId,
                fixtureId: cmd.FixtureId,
                userId: _currentUser.Id,
                homeScore: cmd.HomeScore,
                awayScore: cmd.AwayScore,
                halfTimeHomeScore: cmd.HalfTimeHomeScore,
                halfTimeAwayScore: cmd.HalfTimeAwayScore,
                btts: cmd.BothTeamsToScore,
                overUnder: cmd.OverUnder,
                cards: cmd.YellowCards,
                corners: cmd.Corners,
                firstGoalMinute: cmd.FirstGoalMinute,
                mvpPlayerId: cmd.MvpPlayerId,
                isDraft: !cmd.Submit
            );
            UnitOfWork.MatchPredictions.Add(prediction);
        }
        else
        {
            prediction.UpdateDraft(
                homeScore: cmd.HomeScore,
                awayScore: cmd.AwayScore,
                halfTimeHomeScore: cmd.HalfTimeHomeScore,
                halfTimeAwayScore: cmd.HalfTimeAwayScore,
                btts: cmd.BothTeamsToScore,
                overUnder: cmd.OverUnder,
                cards: cmd.YellowCards,
                corners: cmd.Corners,
                firstGoalMinute: cmd.FirstGoalMinute,
                mvpPlayerId: cmd.MvpPlayerId,
                submit: cmd.Submit,
                kickoffAt: fixture.KickoffAt
            );
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
