using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPlayerMatchStatByIdQuery(Guid Id) : IQuery<Result<PlayerMatchStatDto>>;

public sealed class GetPlayerMatchStatByIdQueryHandler : BaseQueryHandler<GetPlayerMatchStatByIdQuery, Result<PlayerMatchStatDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayerMatchStatByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayerMatchStatByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<PlayerMatchStatDto>> ExecuteAsync(GetPlayerMatchStatByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _uow.PlayerMatchStats.GetByIdAsync(request.Id);
        if (s == null) return Result<PlayerMatchStatDto>.Failure("PlayerMatchStat not found.");

        var player = await _uow.Players.GetByIdAsync(s.PlayerId);
        var team = await _uow.Teams.GetByIdAsync(s.TeamId);
        var fixture = await _uow.Fixtures.GetByIdAsync(s.FixtureId);

        return Result<PlayerMatchStatDto>.Success(new PlayerMatchStatDto(
            s.Id,
            s.FixtureId,
            $"{fixture?.Round ?? "Match"} ({fixture?.KickoffAt:yyyy-MM-dd})",
            s.PlayerId,
            player?.DisplayName ?? "Player",
            s.TeamId,
            team?.Name ?? "Team",
            s.IsStarter,
            s.MinutesPlayed,
            s.Goals,
            s.Assists,
            s.OwnGoals,
            s.ShotsTotal,
            s.ShotsOnTarget,
            s.PassesTotal,
            s.KeyPasses,
            s.Tackles,
            s.Interceptions,
            s.Saves,
            s.GoalsConceded,
            s.PenaltiesSaved,
            s.PenaltiesMissed,
            s.YellowCards,
            s.RedCards,
            s.CleanSheet,
            s.ProviderRating
        ));
    }
}
