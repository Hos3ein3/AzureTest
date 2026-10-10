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
using FluentValidation;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPredictClubDetailsQuery(Guid PredictClubId) : IQuery<Result<PredictClubDetailsDto>>;

public record PredictClubDetailsDto(
    PredictClubDto Club,
    List<PredictClubMemberDto> Members,
    List<MatchPredictionDto> UpcomingFixtures,
    TournamentPredictionDto? UserTournamentPrediction,
    bool IsUserMember,
    bool IsUserOwner
);

public sealed class GetPredictClubDetailsQueryValidator : AbstractValidator<GetPredictClubDetailsQuery>
{
    public GetPredictClubDetailsQueryValidator()
    {
        RuleFor(x => x.PredictClubId).NotEmpty();
    }
}

public sealed class GetPredictClubDetailsQueryHandler : IQueryHandler<GetPredictClubDetailsQuery, Result<PredictClubDetailsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GetPredictClubDetailsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<PredictClubDetailsDto>> HandleAsync(GetPredictClubDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var club = await _uow.PredictClubs.GetWithDetailsAsync(query.PredictClubId, cancellationToken);
        if (club == null) return Result<PredictClubDetailsDto>.Failure("Predict Club not found.");

        var members = await _uow.PredictClubMembers.FindAsync(m => m.PredictClubId == query.PredictClubId && m.Status == FantasyMemberStatus.Active);

        int rank = 1;
        var memberDtos = members
            .OrderByDescending(m => m.TotalPoints)
            .ThenByDescending(m => m.CorrectExactScores)
            .Select(m =>
            {
                return new PredictClubMemberDto(
                    m.Id,
                    m.PredictClubId,
                    m.UserId,
                    "Predictor " + m.UserId.ToString().Substring(0, 5),
                    m.Role,
                    m.Status,
                    m.JoinedAt,
                    m.TotalPoints,
                    m.CorrectExactScores,
                    m.CorrectOutcomes,
                    m.TotalPredictionsCount,
                    rank++
                );
            })
            .ToList();

        // Get upcoming fixtures
        var allFixtures = await _uow.Fixtures.GetAllAsync();
        var allTeams = await _uow.Teams.GetAllAsync();
        var userPredictions = await _uow.MatchPredictions.FindAsync(p => p.PredictClubId == query.PredictClubId && p.UserId == _currentUser.Id);

        var upcoming = allFixtures
            .Where(f => f.KickoffAt >= DateTime.UtcNow.AddHours(-3)) // Show recent and upcoming
            .OrderBy(f => f.KickoffAt)
            .Take(20)
            .Select(f =>
            {
                var home = allTeams.FirstOrDefault(t => t.Id == f.HomeTeamId);
                var away = allTeams.FirstOrDefault(t => t.Id == f.AwayTeamId);
                var userPred = userPredictions.FirstOrDefault(p => p.FixtureId == f.Id);

                bool canEdit = DateTime.UtcNow < f.KickoffAt.AddMinutes(-1);

                return new MatchPredictionDto(
                    userPred?.Id ?? Guid.Empty,
                    club.Id,
                    f.Id,
                    home?.Name ?? "Home",
                    home?.LogoUrl,
                    away?.Name ?? "Away",
                    away?.LogoUrl,
                    f.KickoffAt,
                    f.Round,
                    f.HomeScore,
                    f.AwayScore,
                    f.Status,
                    _currentUser.Id,
                    userPred?.Status ?? PredictionEntryStatus.Draft,
                    userPred?.PredictedHomeScore,
                    userPred?.PredictedAwayScore,
                    userPred?.PredictedHalfTimeHomeScore,
                    userPred?.PredictedHalfTimeAwayScore,
                    userPred?.BothTeamsToScore,
                    userPred?.TotalGoalsOverUnder,
                    userPred?.PredictedTotalYellowCards,
                    userPred?.PredictedTotalCorners,
                    userPred?.PredictedFirstGoalMinute,
                    userPred?.PredictedMvpPlayerId,
                    null,
                    userPred?.PointsEarned ?? 0,
                    userPred?.IsScored ?? false,
                    canEdit
                );
            })
            .ToList();

        var tournamentPredList = await _uow.TournamentPredictions.FindAsync(p => p.PredictClubId == query.PredictClubId && p.UserId == _currentUser.Id);
        var userTournamentPred = tournamentPredList.FirstOrDefault();

        TournamentPredictionDto? tournamentDto = null;
        if (userTournamentPred != null)
        {
            var firstTeam = allTeams.FirstOrDefault(t => t.Id == userTournamentPred.FirstPlaceTeamId);
            var secondTeam = allTeams.FirstOrDefault(t => t.Id == userTournamentPred.SecondPlaceTeamId);
            var thirdTeam = allTeams.FirstOrDefault(t => t.Id == userTournamentPred.ThirdPlaceTeamId);

            tournamentDto = new TournamentPredictionDto(
                userTournamentPred.Id,
                club.Id,
                _currentUser.Id,
                userTournamentPred.Status,
                userTournamentPred.FirstPlaceTeamId,
                firstTeam?.Name,
                userTournamentPred.SecondPlaceTeamId,
                secondTeam?.Name,
                userTournamentPred.ThirdPlaceTeamId,
                thirdTeam?.Name,
                userTournamentPred.BestPlayerId,
                null,
                userTournamentPred.TopScorerPlayerId,
                null,
                userTournamentPred.TopAssisterPlayerId,
                null,
                userTournamentPred.PointsEarned,
                userTournamentPred.IsScored
            );
        }

        var clubDto = new PredictClubDto(
            club.Id,
            club.OwnerUserId,
            club.Name,
            club.ClubTag,
            club.Description,
            club.LogoUrl,
            club.Visibility,
            club.Format,
            club.ScopeType,
            club.ScopeValue,
            club.MaxMembers,
            memberDtos.Count,
            club.JoinCode,
            club.Status,
            club.StartsAt,
            club.EndsAt,
            club.ExactScorePoints,
            club.OutcomePoints,
            club.HalfTimeScorePoints,
            club.BothTeamsToScorePoints,
            club.OverUnderPoints,
            club.YellowCardsPoints,
            club.CornersPoints,
            club.MatchMvpPoints,
            club.FirstGoalMinutePoints,
            club.EnableTournamentPredictions,
            club.TournamentPodiumPoints,
            club.TournamentAwardPoints,
            club.PredictExactScore,
            club.PredictOutcome,
            club.PredictHalfTime,
            club.PredictBtts,
            club.PredictCards,
            club.PredictCorners,
            club.PredictMvp
        );

        bool isMember = members.Any(m => m.UserId == _currentUser.Id);
        bool isOwner = club.OwnerUserId == _currentUser.Id;

        return Result<PredictClubDetailsDto>.Success(new PredictClubDetailsDto(
            clubDto,
            memberDtos,
            upcoming,
            tournamentDto,
            isMember,
            isOwner
        ));
    }
}
