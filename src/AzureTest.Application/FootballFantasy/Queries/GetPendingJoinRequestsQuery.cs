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
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPendingJoinRequestsQuery(Guid FantasyLeagueId) : IQuery<Result<List<PendingJoinRequestDto>>>;

public sealed class GetPendingJoinRequestsQueryHandler : BaseQueryHandler<GetPendingJoinRequestsQuery, Result<List<PendingJoinRequestDto>>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public GetPendingJoinRequestsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ICurrentUser currentUser, ILogger<GetPendingJoinRequestsQueryHandler> logger)
        : base(logger)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    protected override async Task<Result<List<PendingJoinRequestDto>>> ExecuteAsync(GetPendingJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        var league = await _uow.FantasyLeagues.GetWithDetailsAsync(request.FantasyLeagueId, cancellationToken);
        if (league == null) return Result<List<PendingJoinRequestDto>>.Failure("Fantasy league not found.");

        if (league.OwnerUserId.HasValue && league.OwnerUserId.Value != _currentUser.Id && !_currentUser.IsAdmin)
        {
            return Result<List<PendingJoinRequestDto>>.Failure("Only the league owner or system administrator can inspect pending join requests.");
        }

        // Pending members or unapproved teams
        var pendingMembers = league.Members
            .Where(m => m.Status == FantasyMemberStatus.PendingApproval)
            .ToList();

        var pendingTeams = league.FantasyTeams
            .Where(t => !t.IsApproved)
            .ToDictionary(t => t.UserId);

        var allUsersTeams = await _uow.FantasyTeams.GetAllAsync();

        var list = new List<PendingJoinRequestDto>();

        foreach (var m in pendingMembers)
        {
            pendingTeams.TryGetValue(m.UserId, out var team);
            var teamName = team?.Name ?? "Pending Team";
            var logoUrl = team?.LogoUrl;

            // Career stats for the manager
            var managerTeams = allUsersTeams.Where(t => t.UserId == m.UserId).ToList();
            int careerPoints = managerTeams.Sum(t => t.TotalPoints);
            int trophies = managerTeams.Count(t => t.TotalPoints > 50);

            list.Add(new PendingJoinRequestDto(
                league.Id,
                m.UserId,
                teamName,
                teamName,
                logoUrl,
                m.JoinedAt,
                careerPoints,
                trophies
            ));
        }

        // Also catch any team where IsApproved is false but member status wasn't strictly PendingApproval
        foreach (var kvp in pendingTeams)
        {
            if (pendingMembers.All(m => m.UserId != kvp.Key))
            {
                var team = kvp.Value;
                var managerTeams = allUsersTeams.Where(t => t.UserId == team.UserId).ToList();
                int careerPoints = managerTeams.Sum(t => t.TotalPoints);
                int trophies = managerTeams.Count(t => t.TotalPoints > 50);

                list.Add(new PendingJoinRequestDto(
                    league.Id,
                    team.UserId,
                    team.Name,
                    team.Name,
                    team.LogoUrl,
                    team.CreatedOn,
                    careerPoints,
                    trophies
                ));
            }
        }

        return Result<List<PendingJoinRequestDto>>.Success(list);
    }
}
