using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreatePlayerMatchStatCommand(
    Guid FixtureId,
    Guid PlayerId,
    Guid TeamId,
    bool IsStarter,
    int MinutesPlayed,
    int Goals = 0,
    int Assists = 0,
    int OwnGoals = 0,
    int ShotsTotal = 0,
    int ShotsOnTarget = 0,
    int PassesTotal = 0,
    int KeyPasses = 0,
    int Tackles = 0,
    int Interceptions = 0,
    int Saves = 0,
    int GoalsConceded = 0,
    int PenaltiesSaved = 0,
    int PenaltiesMissed = 0,
    int YellowCards = 0,
    int RedCards = 0,
    bool CleanSheet = false,
    decimal? ProviderRating = null
) : ICommand<Result<Guid>>;

public sealed class CreatePlayerMatchStatCommandValidator : AbstractValidator<CreatePlayerMatchStatCommand>
{
    public CreatePlayerMatchStatCommandValidator()
    {
        RuleFor(x => x.FixtureId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.MinutesPlayed).InclusiveBetween(0, 130);
    }
}

public sealed class CreatePlayerMatchStatCommandHandler : BaseCommandHandler<CreatePlayerMatchStatCommand, Result<Guid>>
{
    public CreatePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerMatchStatCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var stat = PlayerMatchStat.Create(
            request.FixtureId,
            request.PlayerId,
            request.TeamId,
            request.IsStarter,
            request.MinutesPlayed,
            request.Goals,
            request.Assists,
            request.OwnGoals,
            request.ShotsTotal,
            request.ShotsOnTarget,
            request.PassesTotal,
            request.KeyPasses,
            request.Tackles,
            request.Interceptions,
            request.Saves,
            request.GoalsConceded,
            request.PenaltiesSaved,
            request.PenaltiesMissed,
            request.YellowCards,
            request.RedCards,
            request.CleanSheet,
            request.ProviderRating
        );

        UnitOfWork.PlayerMatchStats.Add(stat);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(stat.Id);
    }
}
