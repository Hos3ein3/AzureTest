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

public sealed record GetCountryByIdQuery(Guid Id) : IQuery<Result<CountryDto>>;

public sealed class GetCountryByIdQueryHandler : BaseQueryHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetCountryByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetCountryByIdQueryHandler> logger) : base(logger)
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
