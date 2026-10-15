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
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetCoachesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetCoachesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<CoachDto, CoachFilter>> ExecuteAsync(GetCoachesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new CoachFilter();
        var all = await _uow.Coaches.GetAllAsync();
        var countries = (await _uow.Countries.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);

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

        var list = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => new CoachDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.CountryId,
                countries.GetValueOrDefault(c.CountryId, "Unknown"),
                c.DateOfBirth,
                c.PhotoUrl
            ))
            .ToList();

        return new PagedResult<CoachDto, CoachFilter>(items, total, page, size, filter);
    }
}
