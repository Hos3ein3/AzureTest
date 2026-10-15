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

public sealed record GetFixtureByIdQuery(Guid Id) : IQuery<Result<FixtureDto>>;

public sealed class GetFixtureByIdQueryHandler : BaseQueryHandler<GetFixtureByIdQuery, Result<FixtureDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetFixtureByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetFixtureByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<FixtureDto>> ExecuteAsync(GetFixtureByIdQuery request, CancellationToken cancellationToken)
    {
        var f = await _uow.Fixtures.GetByIdAsync(request.Id);
        if (f == null) return Result<FixtureDto>.Failure("Fixture not found.");

        var homeTeam = await _uow.Teams.GetByIdAsync(f.HomeTeamId);
        var awayTeam = await _uow.Teams.GetByIdAsync(f.AwayTeamId);

        return Result<FixtureDto>.Success(new FixtureDto(
            f.Id,
            f.SeasonId,
            homeTeam?.Name ?? "Home",
            homeTeam?.LogoUrl,
            awayTeam?.Name ?? "Away",
            awayTeam?.LogoUrl,
            f.KickoffAt,
            f.Status,
            f.HomeScore,
            f.AwayScore,
            f.Minute,
            f.Round,
            f.Venue,
            f.IsStatsFinalized
        ));
    }
}
