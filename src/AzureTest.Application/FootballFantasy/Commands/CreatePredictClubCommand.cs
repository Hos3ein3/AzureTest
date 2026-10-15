using System;
using System.Security.Cryptography;
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
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreatePredictClubCommand(
    string Name,
    DateTime StartsAt,
    DateTime EndsAt,
    FantasyLeagueVisibility Visibility,
    FantasyLeagueFormat Format,
    PoolScopeType ScopeType,
    string ScopeValue,
    string? Description = null,
    string? LogoUrl = null,
    int? MaxMembers = null,
    string? JoinCode = null,
    int ExactScorePoints = PredictClub.Rules.DefaultExactScorePoints,
    int OutcomePoints = PredictClub.Rules.DefaultOutcomePoints,
    int HalfTimeScorePoints = PredictClub.Rules.DefaultFactPoints,
    int BothTeamsToScorePoints = PredictClub.Rules.DefaultFactPoints,
    int OverUnderPoints = PredictClub.Rules.DefaultFactPoints,
    int YellowCardsPoints = PredictClub.Rules.DefaultFactPoints,
    int CornersPoints = PredictClub.Rules.DefaultFactPoints,
    int MatchMvpPoints = PredictClub.Rules.DefaultFactPoints,
    int FirstGoalMinutePoints = PredictClub.Rules.DefaultFactPoints,
    bool EnableTournamentPredictions = true,
    int TournamentPodiumPoints = PredictClub.Rules.DefaultTournamentOutcomePoints,
    int TournamentAwardPoints = PredictClub.Rules.DefaultTournamentOutcomePoints,
    bool PredictExactScore = true,
    bool PredictOutcome = true,
    bool PredictHalfTime = true,
    bool PredictBtts = true,
    bool PredictCards = true,
    bool PredictCorners = true,
    bool PredictMvp = true
) : ICommand<Result<Guid>>;

public sealed class CreatePredictClubCommandValidator : AbstractValidator<CreatePredictClubCommand>
{
    public CreatePredictClubCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(PredictClub.Rules.NameMinLength)
            .MaximumLength(PredictClub.Rules.NameMaxLength);

        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt);

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description).MaximumLength(PredictClub.Rules.DescriptionMaxLength);
        });

        When(x => !string.IsNullOrEmpty(x.LogoUrl), () =>
        {
            RuleFor(x => x.LogoUrl).MaximumLength(PredictClub.Rules.LogoUrlMaxLength);
        });

        When(x => x.Visibility == FantasyLeagueVisibility.Private, () =>
        {
            RuleFor(x => x.JoinCode)
                .NotEmpty()
                .MaximumLength(PredictClub.Rules.JoinCodeMaxLength);
        });
    }
}

public sealed class CreatePredictClubCommandHandler : BaseCommandHandler<CreatePredictClubCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;

    public CreatePredictClubCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, ILogger<CreatePredictClubCommandHandler> logger)
        : base(uow, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePredictClubCommand cmd, CancellationToken cancellationToken)
    {
        // Generate random 10-character club tag (numbers and uppercase letters)
        string clubTag = GenerateClubTag();

        var club = PredictClub.Create(
            ownerUserId: _currentUser.Id,
            name: cmd.Name,
            clubTag: clubTag,
            startsAt: cmd.StartsAt.ToUtc(),
            endsAt: cmd.EndsAt.ToUtc(),
            visibility: cmd.Visibility,
            format: cmd.Format,
            scopeType: cmd.ScopeType,
            scopeValue: cmd.ScopeValue,
            description: cmd.Description,
            logoUrl: cmd.LogoUrl,
            maxMembers: cmd.MaxMembers,
            joinCode: cmd.JoinCode,
            exactScorePoints: cmd.ExactScorePoints,
            outcomePoints: cmd.OutcomePoints,
            halfTimeScorePoints: cmd.HalfTimeScorePoints,
            bothTeamsToScorePoints: cmd.BothTeamsToScorePoints,
            overUnderPoints: cmd.OverUnderPoints,
            yellowCardsPoints: cmd.YellowCardsPoints,
            cornersPoints: cmd.CornersPoints,
            matchMvpPoints: cmd.MatchMvpPoints,
            firstGoalMinutePoints: cmd.FirstGoalMinutePoints,
            enableTournamentPredictions: cmd.EnableTournamentPredictions,
            tournamentPodiumPoints: cmd.TournamentPodiumPoints,
            tournamentAwardPoints: cmd.TournamentAwardPoints,
            predictExactScore: cmd.PredictExactScore,
            predictOutcome: cmd.PredictOutcome,
            predictHalfTime: cmd.PredictHalfTime,
            predictBtts: cmd.PredictBtts,
            predictCards: cmd.PredictCards,
            predictCorners: cmd.PredictCorners,
            predictMvp: cmd.PredictMvp
        );

        UnitOfWork.PredictClubs.Add(club);

        // Add creator as Admin member
        var ownerMember = PredictClubMember.Create(
            predictClubId: club.Id,
            userId: _currentUser.Id,
            role: FantasyMemberRole.Admin,
            status: FantasyMemberStatus.Active
        );
        UnitOfWork.PredictClubMembers.Add(ownerMember);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(club.Id);
    }

    private static string GenerateClubTag()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Span<char> result = stackalloc char[10];
        for (int i = 0; i < 10; i++)
        {
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(result);
    }
}
