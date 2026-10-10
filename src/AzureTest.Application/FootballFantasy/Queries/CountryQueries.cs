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

public sealed record GetCountriesQuery(CountryFilter Filter) : IQuery<PagedResult<CountryDto, CountryFilter>>;

public sealed class GetCountriesQueryHandler : BaseQueryHandler<GetCountriesQuery, PagedResult<CountryDto, CountryFilter>>
{
    private readonly IUnitOfWork _uow;

    public GetCountriesQueryHandler(IUnitOfWork uow, ILogger<GetCountriesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<CountryDto, CountryFilter>> ExecuteAsync(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new CountryFilter();
        var all = await _uow.Countries.GetAllAsync();
        IEnumerable<Country> query = all;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var s = filter.SearchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || c.Code.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.Continent))
        {
            query = query.Where(c => c.Continent.Equals(filter.Continent, StringComparison.OrdinalIgnoreCase));
        }

        var list = query.ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list
            .OrderBy(c => c.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => new CountryDto(c.Id, c.Name, c.Code, c.Continent, c.FlagUrl))
            .ToList();

        return new PagedResult<CountryDto, CountryFilter>(items, total, page, size, filter);
    }
}

public sealed record GetCountryByIdQuery(Guid Id) : IQuery<Result<CountryDto>>;

public sealed class GetCountryByIdQueryHandler : BaseQueryHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCountryByIdQueryHandler(IUnitOfWork uow, ILogger<GetCountryByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<CountryDto>> ExecuteAsync(GetCountryByIdQuery request, CancellationToken cancellationToken)
    {
        var country = await _uow.Countries.GetByIdAsync(request.Id);
        if (country == null) return Result<CountryDto>.Failure("Country not found.");
        return Result<CountryDto>.Success(new CountryDto(country.Id, country.Name, country.Code, country.Continent, country.FlagUrl));
    }
}
