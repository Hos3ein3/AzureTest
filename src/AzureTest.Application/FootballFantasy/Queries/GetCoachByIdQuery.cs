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

public sealed record GetCoachByIdQuery(Guid Id) : IQuery<Result<CoachDto>>;

public sealed class GetCoachByIdQueryHandler : BaseQueryHandler<GetCoachByIdQuery, Result<CoachDto>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetCoachByIdQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetCoachByIdQueryHandler> logger) : base(logger)
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
