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

public sealed record GetPlayerInjuriesQuery(PlayerInjuryFilter Filter) : IQuery<PagedResult<PlayerInjuryDto, PlayerInjuryFilter>>;

public sealed class GetPlayerInjuriesQueryHandler : BaseQueryHandler<GetPlayerInjuriesQuery, PagedResult<PlayerInjuryDto, PlayerInjuryFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayerInjuriesQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayerInjuriesQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerInjuryDto, PlayerInjuryFilter>> ExecuteAsync(GetPlayerInjuriesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerInjuryFilter();
        var all = await _uow.PlayerInjuries.GetAllAsync();
        var players = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.Id, p => p.DisplayName);

        IEnumerable<PlayerInjury> query = all;
        if (filter.PlayerId.HasValue) query = query.Where(x => x.PlayerId == filter.PlayerId.Value);
        if (filter.Severity.HasValue) query = query.Where(x => x.Severity == filter.Severity.Value);

        var list = query.OrderByDescending(x => x.StartDate).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(i => new PlayerInjuryDto(
                i.Id,
                i.PlayerId,
                players.GetValueOrDefault(i.PlayerId, "Player"),
                i.FixtureId,
                i.Type,
                i.Reason,
                i.Severity,
                i.StartDate,
                i.ExpectedReturnDate
            )).ToList();

        return new PagedResult<PlayerInjuryDto, PlayerInjuryFilter>(items, total, page, size, filter);
    }
}
