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

public sealed record GetCoachesQuery(CoachFilter Filter) : IQuery<PagedResult<CoachDto, CoachFilter>>;

public sealed class GetCoachesQueryHandler : BaseQueryHandler<GetCoachesQuery, PagedResult<CoachDto, CoachFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetCoachesQueryHandler(IUnitOfWork uow, ILogger<GetCoachesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<CoachDto, CoachFilter>> ExecuteAsync(GetCoachesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new CoachFilter();
        var all = await _uow.Coaches.GetAllAsync();
        var countries = await _uow.Countries.GetAllAsync();
        var countryMap = countries.ToDictionary(c => c.Id, c => c.Name);

        IEnumerable<Coach> query = all;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(c => c.FirstName.ToLower().Contains(s) || c.LastName.ToLower().Contains(s));
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(c => c.CountryId == filter.CountryId.Value);
        }

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderBy(c => c.LastName)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => new CoachDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.CountryId,
                countryMap.TryGetValue(c.CountryId, out var cName) ? cName : "Unknown",
                c.DateOfBirth,
                c.PhotoUrl
            ))
            .ToList();

        return new PagedResult<CoachDto, CoachFilter>(items, total, page, size, filter);
    }
}

public sealed record GetCoachByIdQuery(Guid Id) : IQuery<Result<CoachDto>>;

public sealed class GetCoachByIdQueryHandler : BaseQueryHandler<GetCoachByIdQuery, Result<CoachDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCoachByIdQueryHandler(IUnitOfWork uow, ILogger<GetCoachByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<CoachDto>> ExecuteAsync(GetCoachByIdQuery request, CancellationToken cancellationToken)
    {
        var coach = await _uow.Coaches.GetByIdAsync(request.Id);
        if (coach == null) return Result<CoachDto>.Failure("Coach not found.");

        var country = await _uow.Countries.GetByIdAsync(coach.CountryId);

        return Result<CoachDto>.Success(new CoachDto(
            coach.Id,
            coach.FirstName,
            coach.LastName,
            coach.CountryId,
            country?.Name ?? "Unknown",
            coach.DateOfBirth,
            coach.PhotoUrl
        ));
    }
}
