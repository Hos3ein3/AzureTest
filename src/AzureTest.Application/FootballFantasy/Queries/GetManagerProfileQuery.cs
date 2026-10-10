using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetManagerProfileQuery(Guid UserId) : IQuery<Result<ManagerProfileDto>>;

public sealed class GetManagerProfileQueryHandler : IQueryHandler<GetManagerProfileQuery, Result<ManagerProfileDto>>
{
    private readonly IUnitOfWork _uow;

    public GetManagerProfileQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<ManagerProfileDto>> HandleAsync(GetManagerProfileQuery request, CancellationToken cancellationToken)
    {
        // Fetch user's fantasy teams
        var allTeams = await _uow.FantasyTeams.GetAllAsync();
        var userTeams = allTeams.Where(t => t.UserId == request.UserId).ToList();

        // Fetch user's achievements
        var userAchievements = await _uow.UserAchievements.FindAsync(a => a.UserId == request.UserId);
        var allAchievements = await _uow.Achievements.GetAllAsync();
        var achievementMap = allAchievements.ToDictionary(a => a.Id);

        var achievementDtos = userAchievements
            .Where(ua => achievementMap.ContainsKey(ua.AchievementId))
            .Select(ua =>
            {
                var ach = achievementMap[ua.AchievementId];
                return new UserAchievementDto(
                    ua.Id,
                    ua.UserId,
                    ua.AchievementId,
                    ach.Title,
                    ach.Description,
                    ach.BadgeIcon,
                    ach.PointsReward,
                    ua.UnlockedAt
                );
            })
            .ToList();

        var teamDtos = new List<FantasyTeamDto>();
        int totalCareerPoints = 0;

        foreach (var t in userTeams)
        {
            var league = await _uow.FantasyLeagues.GetByIdAsync(t.FantasyLeagueId);
            totalCareerPoints += t.TotalPoints;
            teamDtos.Add(new FantasyTeamDto(
                t.Id,
                t.FantasyLeagueId,
                league?.Name ?? "Fantasy League",
                t.UserId,
                t.Name,
                t.LogoUrl,
                t.CreditsLeft,
                t.TotalPoints
            ));
        }

        // Fetch user's league memberships including left/active
        var memberships = await _uow.FantasyLeagueMembers.FindAsync(m => m.UserId == request.UserId);
        var allLeagues = await _uow.FantasyLeagues.GetAllAsync();
        var leagueMap = allLeagues.ToDictionary(l => l.Id);

        var historyDtos = memberships.Select(m =>
        {
            var l = leagueMap.ContainsKey(m.FantasyLeagueId) ? leagueMap[m.FantasyLeagueId] : null;
            return new ManagerLeagueHistoryDto(
                m.FantasyLeagueId,
                l?.Name ?? "Fantasy League",
                l?.LeagueTag ?? "LEAGUE",
                m.Role,
                m.Status,
                m.JoinedAt,
                m.Status == AzureTest.Domain.Entities.FootballFantasy.Enums.FantasyMemberStatus.Left
            );
        }).OrderByDescending(h => h.JoinedAt).ToList();

        var profileDto = new ManagerProfileDto(
            request.UserId,
            userTeams.FirstOrDefault()?.Name != null ? $"Manager ({userTeams.First().Name})" : "Fantasy Manager",
            string.Empty,
            totalCareerPoints,
            userTeams.Count,
            userTeams.Count(t => t.TotalPoints > 50),
            teamDtos,
            achievementDtos,
            historyDtos
        );

        return Result<ManagerProfileDto>.Success(profileDto);
    }
}
