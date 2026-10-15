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

public sealed record GetTeamByIdQuery(Guid Id) : IQuery<Result<TeamDto>>;

public sealed class GetTeamByIdQueryHandler : BaseQueryHandler<GetTeamByIdQuery, Result<TeamDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetTeamByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetTeamByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<TeamDto>> ExecuteAsync(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _uow.Teams.GetByIdAsync(request.Id);
        if (team == null) return Result<TeamDto>.Failure("Team not found.");

        var country = await _uow.Countries.GetByIdAsync(team.CountryId);
        string? venueName = null;
        if (team.VenueId.HasValue)
        {
            var venue = await _uow.Venues.GetByIdAsync(team.VenueId.Value);
            venueName = venue?.Name;
        }

        return Result<TeamDto>.Success(new TeamDto(
            team.Id,
            team.Name,
            team.ShortName,
            team.Code,
            team.TeamType,
            team.CountryId,
            country?.Name ?? "Unknown",
            team.FoundedYear,
            team.StadiumName,
            team.StadiumCapacity,
            team.LogoUrl,
            team.VenueId,
            venueName
        ));
    }
}
