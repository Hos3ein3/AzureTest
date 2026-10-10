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

public sealed record GetLeaguesQuery(LeagueFilter Filter) : IQuery<PagedResult<LeagueDto, LeagueFilter>>;

public sealed class GetLeaguesQueryHandler : BaseQueryHandler<GetLeaguesQuery, PagedResult<LeagueDto, LeagueFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetLeaguesQueryHandler(IUnitOfWork uow, ILogger<GetLeaguesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<LeagueDto, LeagueFilter>> ExecuteAsync(GetLeaguesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new LeagueFilter();
        var all = await _uow.Leagues.GetAllAsync();
        var countries = await _uow.Countries.GetAllAsync();
        var countryMap = countries.ToDictionary(c => c.Id, c => c.Name);

        IEnumerable<League> query = all;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(s) || (l.ShortName != null && l.ShortName.ToLower().Contains(s)));
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(l => l.Type == filter.Type.Value);
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(l => l.CountryId == filter.CountryId.Value);
        }

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderBy(l => l.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(l => new LeagueDto(
                l.Id,
                l.Name,
                l.ShortName,
                l.Type,
                l.Level,
                l.CountryId.HasValue && countryMap.TryGetValue(l.CountryId.Value, out var cName) ? cName : null,
                l.Continent,
                l.LogoUrl,
                l.IsActive
            ))
            .ToList();

        return new PagedResult<LeagueDto, LeagueFilter>(items, total, page, size, filter);
    }
}

public sealed record GetLeagueByIdQuery(Guid Id) : IQuery<Result<LeagueDto>>;

public sealed class GetLeagueByIdQueryHandler : BaseQueryHandler<GetLeagueByIdQuery, Result<LeagueDto>>
{
    private readonly IUnitOfWork _uow;

    public GetLeagueByIdQueryHandler(IUnitOfWork uow, ILogger<GetLeagueByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<LeagueDto>> ExecuteAsync(GetLeagueByIdQuery request, CancellationToken cancellationToken)
    {
        var league = await _uow.Leagues.GetByIdAsync(request.Id);
        if (league == null) return Result<LeagueDto>.Failure("League not found.");

        string? countryName = null;
        if (league.CountryId.HasValue)
        {
            var country = await _uow.Countries.GetByIdAsync(league.CountryId.Value);
            countryName = country?.Name;
        }

        return Result<LeagueDto>.Success(new LeagueDto(
            league.Id,
            league.Name,
            league.ShortName,
            league.Type,
            league.Level,
            countryName,
            league.Continent,
            league.LogoUrl,
            league.IsActive
        ));
    }
}
