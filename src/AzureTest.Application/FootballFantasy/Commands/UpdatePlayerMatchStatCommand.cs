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
        RuleFor(x => x.MinutesPlayed).InclusiveBetween(0, 130);
    }
}

public sealed class UpdatePlayerMatchStatCommandHandler : BaseCommandHandler<UpdatePlayerMatchStatCommand, Result>
{
    public UpdatePlayerMatchStatCommandHandler(IUnitOfWork uow, ILogger<UpdatePlayerMatchStatCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdatePlayerMatchStatCommand request, CancellationToken cancellationToken)
    {
        var stat = await UnitOfWork.PlayerMatchStats.GetByIdAsync(request.Id);
        if (stat == null) return Result.Failure("PlayerMatchStat not found.");

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
