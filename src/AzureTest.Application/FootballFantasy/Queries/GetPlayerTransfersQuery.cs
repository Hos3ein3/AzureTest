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

public sealed record GetPlayerTransfersQuery(PlayerTransferFilter Filter) : IQuery<PagedResult<PlayerTransferDto, PlayerTransferFilter>>;

public sealed class GetPlayerTransfersQueryHandler : BaseQueryHandler<GetPlayerTransfersQuery, PagedResult<PlayerTransferDto, PlayerTransferFilter>>
{
    private readonly IReadOnlyFootballFantasyUnitOfWork _uow;

    public GetPlayerTransfersQueryHandler(IReadOnlyFootballFantasyUnitOfWork uow, ILogger<GetPlayerTransfersQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<PagedResult<PlayerTransferDto, PlayerTransferFilter>> ExecuteAsync(GetPlayerTransfersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter ?? new PlayerTransferFilter();
        var all = await _uow.PlayerTransfers.GetAllAsync();
        var players = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.Id, p => p.DisplayName);
        var teams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Id, t => t.Name);

        IEnumerable<PlayerTransfer> query = all;
        if (filter.PlayerId.HasValue) query = query.Where(x => x.PlayerId == filter.PlayerId.Value);
        if (filter.Type.HasValue) query = query.Where(x => x.Type == filter.Type.Value);
        if (filter.TeamId.HasValue)
            query = query.Where(x => x.FromTeamId == filter.TeamId.Value || x.ToTeamId == filter.TeamId.Value);

        var list = query.OrderByDescending(x => x.TransferDate).ToList();
        var total = list.Count;
        var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var size = filter.PageSize < 1 ? 20 : filter.PageSize;

        var items = list.Skip((page - 1) * size).Take(size)
            .Select(t => new PlayerTransferDto(
                t.Id,
                t.PlayerId,
                players.GetValueOrDefault(t.PlayerId, "Player"),
                t.TransferDate,
                t.Type,
                t.FromTeamId,
                t.FromTeamId.HasValue ? teams.GetValueOrDefault(t.FromTeamId.Value) : null,
                t.ToTeamId,
                t.ToTeamId.HasValue ? teams.GetValueOrDefault(t.ToTeamId.Value) : null,
                t.Fee
            )).ToList();

        return new PagedResult<PlayerTransferDto, PlayerTransferFilter>(items, total, page, size, filter);
    }
}
