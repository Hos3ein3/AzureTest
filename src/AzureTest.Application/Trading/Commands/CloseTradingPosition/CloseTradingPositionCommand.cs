using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.CloseTradingPosition;

public sealed record CloseTradingPositionCommand(
    Guid PositionId,
    Guid UserId,
    Guid TenantId,
    decimal? ClosePrice = null) : ICommand<Result<Guid>>;

public sealed class CloseTradingPositionCommandValidator : AbstractValidator<CloseTradingPositionCommand>
{
    public CloseTradingPositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public sealed class CloseTradingPositionCommandHandler : ICommandHandler<CloseTradingPositionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;
    private readonly AzureTest.Application.Interfaces.Services.IAppNotificationService? _notificationService;

    public CloseTradingPositionCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        AzureTest.Application.Interfaces.Services.IAppNotificationService? notificationService = null)
    {
        _uow = uow;
        _marketData = marketData;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> HandleAsync(CloseTradingPositionCommand cmd, CancellationToken ct)
    {
        var position = await _uow.TradingPositions.GetByIdAsync(cmd.PositionId);
        if (position == null || position.Status != TradingPositionStatus.Open || position.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && position.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Active position not found or already closed.");
        }

        decimal exitPrice;
        if (cmd.ClosePrice.HasValue && cmd.ClosePrice.Value > 0)
        {
            exitPrice = cmd.ClosePrice.Value;
        }
        else
        {
            var livePrice = await _marketData.GetCurrentPriceAsync(position.Symbol, 0m, ct);
            if (livePrice > 0)
            {
                exitPrice = livePrice;
            }
            else if (position.CurrentPrice > 0 && position.CurrentPrice != position.EntryPrice)
            {
                exitPrice = position.CurrentPrice;
            }
            else
            {
                var freshQuote = await _marketData.GetQuoteAsync(position.Symbol, null, ct);
                exitPrice = (freshQuote != null && freshQuote.Price > 0)
                    ? freshQuote.Price
                    : (position.CurrentPrice > 0 ? position.CurrentPrice : position.EntryPrice);
            }
        }

        position.Close(exitPrice);

        var account = await _uow.TradingAccounts.GetByIdAsync(position.AccountId);
        if (account != null)
        {
            account.ApplyRealizedPnl(position.RealizedPnl, position.InitialMargin);
        }

        await _uow.SaveChangesAsync(ct);

        if (_notificationService != null)
        {
            var formattedExitPrice = AzureTest.Application.Trading.Services.TradingNotificationFormatter.FormatPrice(exitPrice);
            var formattedPnl = AzureTest.Application.Trading.Services.TradingNotificationFormatter.FormatPnl(position.RealizedPnl);
            var formattedQty = AzureTest.Application.Trading.Services.TradingNotificationFormatter.FormatQuantity(position.Quantity);
            var title = $"Paper Trade Closed: {position.Symbol}";
            var message = $"Closed {position.Side} position for {formattedQty} {position.Symbol} @ {formattedExitPrice}. Realized PnL: {formattedPnl}.";
            
            var metadata = new Dictionary<string, string>
            {
                ["positionId"] = position.Id.ToString(),
                ["symbol"] = position.Symbol,
                ["side"] = position.Side.ToString(),
                ["realizedPnl"] = position.RealizedPnl.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["exitPrice"] = exitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["quantity"] = position.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };

            await _notificationService.SendNotificationAsync(
                position.UserId,
                position.TenantId,
                title,
                message,
                type: "Trade",
                actionUrl: "/Trading",
                metadata: metadata,
                ct: ct);
        }

        return Result<Guid>.Success(position.Id);
    }
}
