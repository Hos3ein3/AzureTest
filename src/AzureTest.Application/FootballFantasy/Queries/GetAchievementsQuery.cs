using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetAchievementsQuery(AchievementFilter? Filter) : IQuery<PagedResult<AchievementDto, AchievementFilter>>;

public sealed class GetAchievementsQueryHandler : BaseQueryHandler<GetAchievementsQuery, PagedResult<AchievementDto, AchievementFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetAchievementsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetAchievementsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<AchievementDto, AchievementFilter>> ExecuteAsync(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new AchievementFilter();
        var all = await _uow.Achievements.GetAllAsync();
        IEnumerable<Achievement> query = all;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(s) || a.Description.ToLower().Contains(s));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == filter.IsActive.Value);
        }

        var list = query.OrderByDescending(a => a.PointsReward).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new AchievementDto(a.Id, a.Title, a.Description, a.BadgeIcon, a.PointsReward, a.IsActive))
            .ToList();

        return new PagedResult<AchievementDto, AchievementFilter>(items, total, page, size, filter);
    }
}
