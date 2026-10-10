using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Queries;

public sealed record GetPlayerByIdQuery(Guid Id) : IQuery<Result<PlayerDto>>;

public sealed class GetPlayerByIdQueryHandler : BaseQueryHandler<GetPlayerByIdQuery, Result<PlayerDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPlayerByIdQueryHandler(IUnitOfWork uow, ILogger<GetPlayerByIdQueryHandler> logger) : base(logger)
    {
        _uow = uow;
    }

    protected override async Task<Result<PlayerDto>> ExecuteAsync(GetPlayerByIdQuery request, CancellationToken cancellationToken)
    {
        var player = await _uow.Players.GetByIdAsync(request.Id);
        if (player == null) return Result<PlayerDto>.Failure("Player not found.");

        var country = await _uow.Countries.GetByIdAsync(player.CountryId);

        // Fetch squads & prices
        var squads = await _uow.PlayerSquads.FindAsync(ps => ps.PlayerId == player.Id && ps.ToDate == null);
        var currentTeam = squads.FirstOrDefault()?.TeamSeason?.Team?.ShortName ?? "Free Agent";

        var prices = await _uow.PlayerPrices.FindAsync(pr => pr.PlayerId == player.Id);
        var price = prices.OrderByDescending(pr => pr.ValidFrom).Select(pr => pr.Price).FirstOrDefault();
        if (price == 0) price = 5.0m;

        return Result<PlayerDto>.Success(new PlayerDto(
            player.Id,
            player.FirstName,
            player.LastName,
            player.DisplayName,
            player.DateOfBirth,
            player.CountryId,
            country?.Name ?? "Unknown",
            player.Position,
            player.PreferredFoot,
            player.HeightCm,
            player.WeightKg,
            player.PhotoUrl,
            player.IsActive,
            currentTeam,
            price,
            0
        ));
    }
}
