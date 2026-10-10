using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
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
        RuleFor(x => x.MinutesPlayed).GreaterThanOrEqualTo(0).LessThanOrEqualTo(150);
        RuleFor(x => x.ProviderRating).InclusiveBetween(PlayerMatchStat.Rules.MinProviderRating, PlayerMatchStat.Rules.MaxProviderRating)
            .When(x => x.ProviderRating.HasValue);
    }
}

public sealed class CreatePlayerMatchStatCommandHandler : BaseCommandHandler<CreatePlayerMatchStatCommand, Result<Guid>>
{
    public CreatePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerMatchStatCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var existing = await UnitOfWork.PlayerMatchStats.FindAsync(pms => pms.FixtureId == request.FixtureId && pms.PlayerId == request.PlayerId);
        if (existing.Count > 0)
            return Result<Guid>.Failure("Match stats for this player in this fixture already exist.");

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

public sealed record UpdatePlayerMatchStatCommand(
    Guid Id,
    bool IsStarter,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int OwnGoals,
    int ShotsTotal,
    int ShotsOnTarget,
    int PassesTotal,
    int KeyPasses,
    int Tackles,
    int Interceptions,
    int Saves,
    int GoalsConceded,
    int PenaltiesSaved,
    int PenaltiesMissed,
    int YellowCards,
    int RedCards,
    bool CleanSheet,
    decimal? ProviderRating
) : ICommand<Result>;

public sealed class UpdatePlayerMatchStatCommandValidator : AbstractValidator<UpdatePlayerMatchStatCommand>
{
    public UpdatePlayerMatchStatCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MinutesPlayed).GreaterThanOrEqualTo(0).LessThanOrEqualTo(150);
        RuleFor(x => x.ProviderRating).InclusiveBetween(PlayerMatchStat.Rules.MinProviderRating, PlayerMatchStat.Rules.MaxProviderRating)
            .When(x => x.ProviderRating.HasValue);
    }
}

public sealed class UpdatePlayerMatchStatCommandHandler : BaseCommandHandler<UpdatePlayerMatchStatCommand, Result>
{
    public UpdatePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<UpdatePlayerMatchStatCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(UpdatePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var stat = await UnitOfWork.PlayerMatchStats.GetByIdAsync(request.Id);
        if (stat == null) return Result.Failure("Player match stat not found.");

        stat.UpdateStats(
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

        UnitOfWork.PlayerMatchStats.Update(stat);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DeletePlayerMatchStatCommand(Guid Id) : ICommand<Result>;

public sealed class DeletePlayerMatchStatCommandHandler : BaseCommandHandler<DeletePlayerMatchStatCommand, Result>
{
    public DeletePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<DeletePlayerMatchStatCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeletePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var stat = await UnitOfWork.PlayerMatchStats.GetByIdAsync(request.Id);
        if (stat == null) return Result.Failure("Player match stat not found.");

        UnitOfWork.PlayerMatchStats.Remove(stat);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
