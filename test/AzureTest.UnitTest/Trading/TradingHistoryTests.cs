using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading.Queries.GetTradingHistory;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Trading;

public class TradingHistoryTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<ITradingAccountRepository> _mockAccounts;
    private readonly Mock<ITradingPositionRepository> _mockPositions;
    private readonly Mock<ITenantProvider> _mockTenantProvider;
    private readonly GetTradingHistoryQueryHandler _handler;

    public TradingHistoryTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockAccounts = new Mock<ITradingAccountRepository>();
        _mockPositions = new Mock<ITradingPositionRepository>();
        _mockTenantProvider = new Mock<ITenantProvider>();

        _mockUow.Setup(u => u.TradingAccounts).Returns(_mockAccounts.Object);
        _mockUow.Setup(u => u.TradingPositions).Returns(_mockPositions.Object);

        _handler = new GetTradingHistoryQueryHandler(_mockUow.Object, _mockTenantProvider.Object);
    }

    [Fact]
    public async Task GetTradingHistory_ReturnsFullTradeDetails_IncludingBalancesAndPrices()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });

        // Position 1: Long BTC closed in profit via Take Profit
        var pos1 = TradingPosition.Open(
            account.Id, _userId, _tenantId, "BTCUSDT", "Bitcoin",
            AssetClass.Crypto, TradingOrderSide.Buy,
            0.1m, 60000m, 10, stopLoss: 58000m, takeProfit: 66000m);
        pos1.Close(66000m); // Realized PnL: (66000 - 60000) * 0.1 = +600

        // Position 2: Short SOL closed in loss via Stop Loss
        var pos2 = TradingPosition.Open(
            account.Id, _userId, _tenantId, "SOLUSDT", "Solana",
            AssetClass.Crypto, TradingOrderSide.Sell,
            10m, 100m, 5, stopLoss: 110m, takeProfit: 80m);
        pos2.Close(110m); // Realized PnL: (100 - 110) * 10 = -100

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { pos1, pos2 });

        var query = new GetTradingHistoryQuery(_userId, _tenantId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Account.CashBalance.Should().Be(10000m);
        dto.Trades.Should().HaveCount(2);

        // Trade 1 (BTC Long) checks
        var btcTrade = dto.Trades.Should().ContainSingle(t => t.Symbol == "BTCUSDT").Subject;
        btcTrade.Side.Should().Be(TradingOrderSide.Buy);
        btcTrade.Quantity.Should().Be(0.1m);
        btcTrade.EntryPrice.Should().Be(60000m);
        btcTrade.ClosePrice.Should().Be(66000m);
        btcTrade.Leverage.Should().Be(10);
        btcTrade.InitialMargin.Should().Be(600m); // (0.1 * 60000) / 10 = 600
        btcTrade.WholeAccountBalance.Should().Be(10000m);
        btcTrade.BalancePercentage.Should().Be(6m); // (600 / 10000) * 100 = 6%
        btcTrade.TakeProfit.Should().Be(66000m);
        btcTrade.StopLoss.Should().Be(58000m);
        btcTrade.RealizedPnl.Should().Be(600m);
        btcTrade.RoiPercentage.Should().Be(100m); // (600 / 600) * 100 = 100%
        btcTrade.OutcomeBadge.Should().Be("WIN");
        btcTrade.CloseReasonBadge.Should().Be("TAKE PROFIT");

        // Trade 2 (SOL Short) checks
        var solTrade = dto.Trades.Should().ContainSingle(t => t.Symbol == "SOLUSDT").Subject;
        solTrade.Side.Should().Be(TradingOrderSide.Sell);
        solTrade.Quantity.Should().Be(10m);
        solTrade.EntryPrice.Should().Be(100m);
        solTrade.ClosePrice.Should().Be(110m);
        solTrade.Leverage.Should().Be(5);
        solTrade.InitialMargin.Should().Be(200m); // (10 * 100) / 5 = 200
        solTrade.TakeProfit.Should().Be(80m);
        solTrade.StopLoss.Should().Be(110m);
        solTrade.RealizedPnl.Should().Be(-100m);
        solTrade.RoiPercentage.Should().Be(-50m); // (-100 / 200) * 100 = -50%
        solTrade.OutcomeBadge.Should().Be("LOSS");
        solTrade.CloseReasonBadge.Should().Be("STOP LOSS");

        // Summary checks
        dto.Summary.TotalTrades.Should().Be(2);
        dto.Summary.WinningTrades.Should().Be(1);
        dto.Summary.LosingTrades.Should().Be(1);
        dto.Summary.WinRatePercentage.Should().Be(50m);
        dto.Summary.TotalRealizedProfit.Should().Be(600m);
        dto.Summary.TotalRealizedLoss.Should().Be(100m);
        dto.Summary.NetRealizedPnl.Should().Be(500m);
        dto.Summary.ProfitFactor.Should().Be(6.0m);
    }

    [Fact]
    public async Task GetTradingHistory_Filters_BySide_And_Outcome()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });

        var pos1 = TradingPosition.Open(account.Id, _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, 0.1m, 60000m, 1);
        pos1.Close(65000m); // Win, Buy

        var pos2 = TradingPosition.Open(account.Id, _userId, _tenantId, "ETHUSDT", "Ethereum", AssetClass.Crypto, TradingOrderSide.Buy, 1m, 3000m, 1);
        pos2.Close(2800m); // Loss, Buy

        var pos3 = TradingPosition.Open(account.Id, _userId, _tenantId, "SOLUSDT", "Solana", AssetClass.Crypto, TradingOrderSide.Sell, 5m, 100m, 1);
        pos3.Close(90m); // Win, Sell

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { pos1, pos2, pos3 });

        // Act 1: Filter only Sell side
        var querySideSell = new GetTradingHistoryQuery(_userId, _tenantId, Side: TradingOrderSide.Sell);
        var resultSideSell = await _handler.HandleAsync(querySideSell, CancellationToken.None);
        resultSideSell.Value.Trades.Should().HaveCount(1);
        resultSideSell.Value.Trades[0].Symbol.Should().Be("SOLUSDT");

        // Act 2: Filter only Wins
        var queryWins = new GetTradingHistoryQuery(_userId, _tenantId, Outcome: "win");
        var resultWins = await _handler.HandleAsync(queryWins, CancellationToken.None);
        resultWins.Value.Trades.Should().HaveCount(2);
        resultWins.Value.Trades.Should().OnlyContain(t => t.RealizedPnl > 0);

        // Act 3: Filter by search
        var querySearch = new GetTradingHistoryQuery(_userId, _tenantId, Search: "Ethereum");
        var resultSearch = await _handler.HandleAsync(querySearch, CancellationToken.None);
        resultSearch.Value.Trades.Should().HaveCount(1);
        resultSearch.Value.Trades[0].Symbol.Should().Be("ETHUSDT");
    }
}
