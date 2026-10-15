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

public sealed record GetVenuesQuery(VenueFilter Filter) : IQuery<PagedResult<VenueDto, VenueFilter>>;

public sealed class GetVenuesQueryHandler : BaseQueryHandler<GetVenuesQuery, PagedResult<VenueDto, VenueFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetVenuesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetVenuesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<VenueDto, VenueFilter>> ExecuteAsync(GetVenuesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new VenueFilter();
        var all = await _uow.Venues.GetAllAsync();
        var countries = (await _uow.Countries.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var teams = await _uow.Teams.GetAllAsync();
        var teamCountByVenue = teams.Where(t => t.VenueId.HasValue).GroupBy(t => t.VenueId!.Value).ToDictionary(g => g.Key, g => g.Count());

        IEnumerable<Venue> query = all;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(v => v.Name.ToLower().Contains(s) || (v.City != null && v.City.ToLower().Contains(s)));
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(v => v.CountryId == filter.CountryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var c = filter.City.ToLower();
            query = query.Where(v => v.City != null && v.City.ToLower().Contains(c));
        }

        var list = query.OrderBy(v => v.Name).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(v => new VenueDto(
                v.Id,
                v.Name,
                v.CountryId,
                countries.GetValueOrDefault(v.CountryId, "Unknown"),
                v.City,
                v.Address,
                v.Capacity,
                v.Surface,
                v.ImageUrl,
                teamCountByVenue.GetValueOrDefault(v.Id, 0)
            ))
            .ToList();

        return new PagedResult<VenueDto, VenueFilter>(items, total, page, size, filter);
    }
}
