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

public sealed record GetLeagueByIdQuery(Guid Id) : IQuery<Result<LeagueDto>>;

public sealed class GetLeagueByIdQueryHandler : BaseQueryHandler<GetLeagueByIdQuery, Result<LeagueDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetLeagueByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetLeagueByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<LeagueDto>> ExecuteAsync(GetLeagueByIdQuery request, CancellationToken cancellationToken)
    {
        var l = await _uow.Leagues.GetByIdAsync(request.Id);
        if (l == null) return Result<LeagueDto>.Failure("League not found.");

        string? countryName = null;
        if (l.CountryId.HasValue)
        {
            var country = await _uow.Countries.GetByIdAsync(l.CountryId.Value);
            countryName = country?.Name;
        }

        return Result<LeagueDto>.Success(new LeagueDto(
            l.Id,
            l.Name,
            l.ShortName,
            l.Type,
            l.Level,
            countryName,
            l.Continent,
            l.LogoUrl,
            l.IsActive
        ));
    }
}
