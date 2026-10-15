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

public sealed record GetAchievementByIdQuery(Guid Id) : IQuery<Result<AchievementDto>>;

public sealed class GetAchievementByIdQueryHandler : BaseQueryHandler<GetAchievementByIdQuery, Result<AchievementDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetAchievementByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetAchievementByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<AchievementDto>> ExecuteAsync(GetAchievementByIdQuery request, CancellationToken cancellationToken)
    {
        var ach = await _uow.Achievements.GetByIdAsync(request.Id);
        if (ach == null) return Result<AchievementDto>.Failure("Achievement not found.");
        return Result<AchievementDto>.Success(new AchievementDto(ach.Id, ach.Title, ach.Description, ach.BadgeIcon, ach.PointsReward, ach.IsActive));
    }
}
