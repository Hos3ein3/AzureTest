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

public sealed record GetFixturesQuery(FixtureFilter Filter) : IQuery<PagedResult<FixtureDto, FixtureFilter>>;

public sealed class GetFixturesQueryHandler : BaseQueryHandler<GetFixturesQuery, PagedResult<FixtureDto, FixtureFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetFixturesQueryHandler(IUnitOfWork uow, ILogger<GetFixturesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<FixtureDto, FixtureFilter>> ExecuteAsync(GetFixturesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new FixtureFilter();
        var all = await _uow.Fixtures.GetAllAsync();
        var teams = await _uow.Teams.GetAllAsync();
        var teamMap = teams.ToDictionary(t => t.Id, t => t);

        IEnumerable<Fixture> query = all;

        if (filter.Status.HasValue)
        {
            query = query.Where(f => f.Status == filter.Status.Value);
        }

        if (filter.SeasonId.HasValue)
        {
            query = query.Where(f => f.SeasonId == filter.SeasonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(f => (f.Venue != null && f.Venue.ToLower().Contains(s))
                                  || (f.Round != null && f.Round.ToLower().Contains(s))
                                  || (teamMap.TryGetValue(f.HomeTeamId, out var ht) && ht.Name.ToLower().Contains(s))
                                  || (teamMap.TryGetValue(f.AwayTeamId, out var at) && at.Name.ToLower().Contains(s)));
        }

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderBy(f => f.KickoffAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(f =>
            {
                var htName = teamMap.TryGetValue(f.HomeTeamId, out var ht) ? ht.Name : "Home";
                var htLogo = ht?.LogoUrl;
                var atName = teamMap.TryGetValue(f.AwayTeamId, out var at) ? at.Name : "Away";
                var atLogo = at?.LogoUrl;

                return new FixtureDto(
                    f.Id,
                    f.SeasonId,
                    htName,
                    htLogo,
                    atName,
                    atLogo,
                    f.KickoffAt,
                    f.Status,
                    f.HomeScore,
                    f.AwayScore,
                    f.Minute,
                    f.Round,
                    f.Venue,
                    f.IsStatsFinalized
                );
            })
            .ToList();

        return new PagedResult<FixtureDto, FixtureFilter>(items, total, page, size, filter);
    }
}

public sealed record GetFixtureByIdQuery(Guid Id) : IQuery<Result<FixtureDto>>;

public sealed class GetFixtureByIdQueryHandler : BaseQueryHandler<GetFixtureByIdQuery, Result<FixtureDto>>
{
    private readonly IUnitOfWork _uow;

    public GetFixtureByIdQueryHandler(IUnitOfWork uow, ILogger<GetFixtureByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<FixtureDto>> ExecuteAsync(GetFixtureByIdQuery request, CancellationToken cancellationToken)
    {
        var f = await _uow.Fixtures.GetByIdAsync(request.Id);
        if (f == null) return Result<FixtureDto>.Failure("Fixture not found.");

        var ht = await _uow.Teams.GetByIdAsync(f.HomeTeamId);
        var at = await _uow.Teams.GetByIdAsync(f.AwayTeamId);

        return Result<FixtureDto>.Success(new FixtureDto(
            f.Id,
            f.SeasonId,
            ht?.Name ?? "Home",
            ht?.LogoUrl,
            at?.Name ?? "Away",
            at?.LogoUrl,
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
