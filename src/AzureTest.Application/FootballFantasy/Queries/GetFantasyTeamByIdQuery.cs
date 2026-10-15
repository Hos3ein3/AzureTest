using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetFantasyTeamByIdQuery(Guid TeamId) : IQuery<Result<FantasyTeamDto>>;

public sealed class GetFantasyTeamByIdQueryHandler : BaseQueryHandler<GetFantasyTeamByIdQuery, Result<FantasyTeamDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetFantasyTeamByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetFantasyTeamByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<FantasyTeamDto>> ExecuteAsync(GetFantasyTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await _uow.FantasyTeams.GetByIdWithDetailsAsync(request.TeamId, cancellationToken);
        if (team == null) return Result<FantasyTeamDto>.Failure("Fantasy team not found.");

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
