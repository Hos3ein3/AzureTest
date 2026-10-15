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

public sealed record GetVenueByIdQuery(Guid Id) : IQuery<Result<VenueDto>>;

public sealed class GetVenueByIdQueryHandler : BaseQueryHandler<GetVenueByIdQuery, Result<VenueDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetVenueByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetVenueByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<VenueDto>> ExecuteAsync(GetVenueByIdQuery request, CancellationToken cancellationToken)
    {
        var v = await _uow.Venues.GetByIdAsync(request.Id);
        if (v == null) return Result<VenueDto>.Failure("Venue not found.");

        var country = await _uow.Countries.GetByIdAsync(v.CountryId);
        var teams = await _uow.Teams.FindAsync(t => t.VenueId == v.Id);

        return Result<VenueDto>.Success(new VenueDto(
            v.Id,
            v.Name,
            v.CountryId,
            country?.Name ?? "Unknown",
            v.City,
            v.Address,
            v.Capacity,
            v.Surface,
            v.ImageUrl,
            teams.Count
        ));
    }
}
