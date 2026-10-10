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

namespace AzureTest.Application.FootballFantasy.Queries;

// GET ACHIEVEMENTS
public sealed record GetAchievementsQuery(AchievementFilter? Filter) : IQuery<PagedResult<AchievementDto, AchievementFilter>>;

public sealed class GetAchievementsQueryHandler : IQueryHandler<GetAchievementsQuery, PagedResult<AchievementDto, AchievementFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetAchievementsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<AchievementDto, AchievementFilter>> HandleAsync(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new AchievementFilter();
        var all = await _uow.Achievements.GetAllAsync();
        var query = all.AsQueryable();

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
        }

        query = filter.SortBy switch
        {
            "Title" => filter.SortDescending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "PointsReward" => filter.SortDescending ? query.OrderByDescending(x => x.PointsReward) : query.OrderBy(x => x.PointsReward),
            _ => filter.SortDescending ? query.OrderByDescending(x => x.CreatedOn) : query.OrderBy(x => x.CreatedOn)
        };

        var total = query.Count();
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AchievementDto(x.Id, x.Title, x.Description, x.BadgeIcon, x.PointsReward, x.IsActive))
            .ToList();

        return new PagedResult<AchievementDto, AchievementFilter>(items, total, pageNumber, pageSize, filter);
    }
}

// GET ACHIEVEMENT BY ID
public sealed record GetAchievementByIdQuery(Guid Id) : IQuery<Result<AchievementDto>>;

public sealed class GetAchievementByIdQueryHandler : IQueryHandler<GetAchievementByIdQuery, Result<AchievementDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAchievementByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<AchievementDto>> HandleAsync(GetAchievementByIdQuery request, CancellationToken cancellationToken)
    {
        var achievement = await _uow.Achievements.GetByIdAsync(request.Id);
        if (achievement == null) return Result<AchievementDto>.Failure("Achievement not found.");

        return Result<AchievementDto>.Success(new AchievementDto(
            achievement.Id,
            achievement.Title,
            achievement.Description,
            achievement.BadgeIcon,
            achievement.PointsReward,
            achievement.IsActive));
    }
}
