using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetFantasyTeamByLeagueQuery(Guid LeagueId, Guid UserId) : IQuery<Result<FantasyTeamDto>>;

public sealed class GetFantasyTeamByLeagueQueryHandler : BaseQueryHandler<GetFantasyTeamByLeagueQuery, Result<FantasyTeamDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetFantasyTeamByLeagueQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetFantasyTeamByLeagueQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<FantasyTeamDto>> ExecuteAsync(GetFantasyTeamByLeagueQuery request, CancellationToken cancellationToken)
    {
        var team = await _uow.FantasyTeams.GetWithLineupsAndSquadAsync(request.LeagueId, request.UserId, cancellationToken);
        if (team == null) return Result<FantasyTeamDto>.Failure("Fantasy team not found for this user in this league.");

        string? coachName = null;
        if (team.CoachId.HasValue)
        {
            var coach = await _uow.Coaches.GetByIdAsync(team.CoachId.Value);
            if (coach != null) coachName = $"{coach.FirstName} {coach.LastName}";
        }

        var dto = new FantasyTeamDto(
            team.Id,
            team.FantasyLeagueId,
            team.FantasyLeague?.Name ?? string.Empty,
            team.UserId,
            team.Name,
            team.LogoUrl,
            team.CreditsLeft,
            team.TotalPoints,
            1,
            team.CoachId,
            coachName,
            team.IsApproved
        );

        return Result<FantasyTeamDto>.Success(dto);
    }
}
