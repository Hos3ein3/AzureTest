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

public sealed record GetTeamsQuery(TeamFilter Filter) : IQuery<PagedResult<TeamDto, TeamFilter>>;

public sealed class GetTeamsQueryHandler : BaseQueryHandler<GetTeamsQuery, PagedResult<TeamDto, TeamFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetTeamsQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetTeamsQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<TeamDto, TeamFilter>> ExecuteAsync(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new TeamFilter();
        var all = await _uow.Teams.GetAllAsync();
        var countries = (await _uow.Countries.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var venues = (await _uow.Venues.GetAllAsync()).ToDictionary(v => v.Id, v => v.Name);

        IEnumerable<Team> query = all;
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s) || t.ShortName.ToLower().Contains(s));
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(t => t.CountryId == filter.CountryId.Value);
        }

        if (filter.VenueId.HasValue)
        {
            query = query.Where(t => t.VenueId == filter.VenueId.Value);
        }

        var list = query.OrderBy(t => t.Name).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.ShortName,
                t.Code,
                t.TeamType,
                t.CountryId,
                countries.GetValueOrDefault(t.CountryId, "Unknown"),
                t.FoundedYear,
                t.StadiumName,
                t.StadiumCapacity,
                t.LogoUrl,
                t.VenueId,
                t.VenueId.HasValue ? venues.GetValueOrDefault(t.VenueId.Value) : null
            ))
            .ToList();

        return new PagedResult<TeamDto, TeamFilter>(items, total, page, size, filter);
    }
}
