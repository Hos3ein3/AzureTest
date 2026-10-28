using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Application.Trading.Commands.ModifyTradingPosition;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Trading;

public class TradingExecutionAndBalanceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<ITradingAccountRepository> _mockAccounts;
    private readonly Mock<ITradingPositionRepository> _mockPositions;
    private readonly Mock<ITradingOrderRepository> _mockOrders;
    private readonly Mock<ITradingMarketDataService> _mockMarketData;

    public TradingExecutionAndBalanceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockAccounts = new Mock<ITradingAccountRepository>();
        _mockPositions = new Mock<ITradingPositionRepository>();
        _mockOrders = new Mock<ITradingOrderRepository>();
        _mockMarketData = new Mock<ITradingMarketDataService>();

        _mockUow.Setup(u => u.TradingAccounts).Returns(_mockAccounts.Object);
        _mockUow.Setup(u => u.TradingPositions).Returns(_mockPositions.Object);
        _mockUow.Setup(u => u.TradingOrders).Returns(_mockOrders.Object);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task PlaceBuyOrder_Market_DeductsMargin_And_CreatesOpenPosition()
    {
        // Arrange: User has $10,000 cash balance
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });

        // Real market price is 0.108 for XANUSDT
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.108m);

        TradingPosition? capturedPosition = null;
        _mockPositions.Setup(p => p.Add(It.IsAny<TradingPosition>()))
            .Callback<TradingPosition>(p => capturedPosition = p);

        var handler = new PlaceTradingOrderCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var cmd = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "XANUSDT",
            AssetName: "XAN / USDT",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 10000m,
            Leverage: 10,
            StopLoss: 0.106m,
            TakeProfit: 0.115m);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedPosition.Should().NotBeNull();
        capturedPosition!.Side.Should().Be(TradingOrderSide.Buy);
        capturedPosition.EntryPrice.Should().Be(0.108m);
        capturedPosition.Quantity.Should().Be(10000m);
        // Notional = 10,000 * 0.108 = 1,080. With 10x leverage, Initial Margin = 108.
        capturedPosition.InitialMargin.Should().Be(108m);
        capturedPosition.Status.Should().Be(TradingPositionStatus.Open);

        // Margin Used is deducted from account
        account.MarginUsed.Should().Be(108m);
        account.FreeMargin.Should().Be(10000m - 108m);
        account.CashBalance.Should().Be(10000m);
    }

    [Fact]
    public async Task PlaceSellOrder_Market_DeductsMargin_And_CreatesOpenShortPosition()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 5000m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });

        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.108m);

        TradingPosition? capturedPosition = null;
        _mockPositions.Setup(p => p.Add(It.IsAny<TradingPosition>()))
            .Callback<TradingPosition>(p => capturedPosition = p);

        var handler = new PlaceTradingOrderCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var cmd = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "XANUSDT",
            AssetName: "XAN / USDT",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Sell,
            OrderType: TradingOrderType.Market,
            Quantity: 5000m,
            Leverage: 10,
            StopLoss: 0.112m,
            TakeProfit: 0.100m);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedPosition.Should().NotBeNull();
        capturedPosition!.Side.Should().Be(TradingOrderSide.Sell);
        // Notional = 5,000 * 0.108 = 540. Margin = 54.
        capturedPosition.InitialMargin.Should().Be(54m);
        account.MarginUsed.Should().Be(54m);
        account.FreeMargin.Should().Be(5000m - 54m);
    }

    [Fact]
    public async Task PlaceOrder_Fails_When_InsufficientFreeMargin()
    {
        // Arrange: User has only $50 balance
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 50m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });

        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("BTCUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(50000m);

        var handler = new PlaceTradingOrderCommandHandler(_mockUow.Object, _mockMarketData.Object);
        // 1 BTC @ $50,000 with 10x leverage needs $5,000 margin
        var cmd = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "BTCUSDT",
            AssetName: "Bitcoin",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 1m,
            Leverage: 10);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient free paper margin");
    }

    [Fact]
    public async Task ManualClosePosition_WithLoss_UpdatesCashBalance_And_ReleasesMargin()
    {
        // User scenario: bought at 0.108, closed at 0.105 with loss
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 10000m,
            entryPrice: 0.108m,
            leverage: 10);

        account.DeductMargin(position.InitialMargin); // MarginUsed = 108
        account.MarginUsed.Should().Be(108m);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        var handler = new CloseTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var cmd = new CloseTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            ClosePrice: 0.105m); // manual close price supplied from client

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.Status.Should().Be(TradingPositionStatus.Closed);
        position.ClosePrice.Should().Be(0.105m);
        // PnL = (0.105 - 0.108) * 10,000 = -30.00
        position.RealizedPnl.Should().Be(-30m);

        // Margin must be completely released back to 0
        account.MarginUsed.Should().Be(0m);
        // Cash Balance must decrease by Realized PnL: $10,000 - $30 = $9,970
        account.CashBalance.Should().Be(9970m);
        account.FreeMargin.Should().Be(9970m);
        account.RealizedPnl.Should().Be(-30m);
    }

    [Fact]
    public async Task ManualClosePosition_WithProfit_IncreasesCashBalance_And_ReleasesMargin()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 10000m,
            entryPrice: 0.108m,
            leverage: 10);

        account.DeductMargin(position.InitialMargin); // 108

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        var handler = new CloseTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var cmd = new CloseTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            ClosePrice: 0.120m); // manual close at profit

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.Status.Should().Be(TradingPositionStatus.Closed);
        // PnL = (0.120 - 0.108) * 10,000 = +120.00
        position.RealizedPnl.Should().Be(120m);

        account.MarginUsed.Should().Be(0m);
        account.CashBalance.Should().Be(10120m);
        account.FreeMargin.Should().Be(10120m);
        account.RealizedPnl.Should().Be(120m);
    }

    [Fact]
    public async Task StopLoss_AutoClosesPosition_WhenPriceHitsOrDropsBelowStopLoss()
    {
        // User scenario: bought at 0.108, SL at 0.106, price dropped to 0.105
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 10000m,
            entryPrice: 0.108m,
            leverage: 10,
            stopLoss: 0.106m);

        account.DeductMargin(position.InitialMargin);

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { position });
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        // Price feeds report 0.105 (below stop loss 0.106)
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.105m);

        _mockOrders.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<TradingOrder>());

        var handler = new SyncTradingPricesCommandHandler(_mockUow.Object, _mockMarketData.Object);

        // Act
        var result = await handler.HandleAsync(new SyncTradingPricesCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1); // 1 position closed
        position.Status.Should().Be(TradingPositionStatus.Closed);
        position.ClosePrice.Should().Be(0.106m); // Executed at stop loss trigger price

        // Realized PnL = (0.106 - 0.108) * 10,000 = -20.00
        position.RealizedPnl.Should().Be(-20m);
        account.MarginUsed.Should().Be(0m);
        account.CashBalance.Should().Be(9980m);
        account.FreeMargin.Should().Be(9980m);
    }

    [Fact]
    public async Task TakeProfit_AutoClosesPosition_WhenPriceHitsOrExceedsTakeProfit()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 10000m,
            entryPrice: 0.108m,
            leverage: 10,
            takeProfit: 0.115m);

        account.DeductMargin(position.InitialMargin);

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { position });
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        // Price feeds report 0.116 (above take profit 0.115)
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.116m);

        _mockOrders.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<TradingOrder>());

        var handler = new SyncTradingPricesCommandHandler(_mockUow.Object, _mockMarketData.Object);

        // Act
        var result = await handler.HandleAsync(new SyncTradingPricesCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        position.Status.Should().Be(TradingPositionStatus.Closed);
        position.ClosePrice.Should().Be(0.115m); // Executed at take profit price

        // Realized PnL = (0.115 - 0.108) * 10,000 = +70.00
        position.RealizedPnl.Should().Be(70m);
        account.MarginUsed.Should().Be(0m);
        account.CashBalance.Should().Be(10070m);
        account.FreeMargin.Should().Be(10070m);
    }

    [Fact]
    public async Task ShortPosition_StopLoss_AutoCloses_WhenPriceRisesAboveStopLoss()
    {
        // Arrange: Short entered at 0.108, SL at 0.112
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Sell,
            quantity: 10000m,
            entryPrice: 0.108m,
            leverage: 10,
            stopLoss: 0.112m);

        account.DeductMargin(position.InitialMargin);

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { position });
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        // Market price rises against short to 0.113 >= 0.112
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.113m);

        _mockOrders.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<TradingOrder>());

        var handler = new SyncTradingPricesCommandHandler(_mockUow.Object, _mockMarketData.Object);

        // Act
        var result = await handler.HandleAsync(new SyncTradingPricesCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        position.Status.Should().Be(TradingPositionStatus.Closed);
        position.ClosePrice.Should().Be(0.112m);
        // Short PnL = (0.108 - 0.112) * 10,000 = -40.00
        position.RealizedPnl.Should().Be(-40m);
        account.MarginUsed.Should().Be(0m);
        account.CashBalance.Should().Be(9960m);
    }

    [Fact]
    public async Task MultiLevelTakeProfit_Validation_EnforcesPercentagesSumAndDirection()
    {
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 10000m);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.108m);

        var handler = new PlaceTradingOrderCommandHandler(_mockUow.Object, _mockMarketData.Object);

        // Case 1: Total percentage > 100%
        var invalidSumJson = "[{\"price\":0.115,\"percentage\":60},{\"price\":0.120,\"percentage\":50}]";
        var cmd1 = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "XANUSDT",
            AssetName: "XAN / USDT",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 1000m,
            MultipleTakeProfitsJson: invalidSumJson);

        var result1 = await handler.HandleAsync(cmd1, CancellationToken.None);
        result1.IsSuccess.Should().BeFalse();
        result1.Error.Should().Contain("cannot exceed 100%");

        // Case 2: Buy order with TP price below entry price
        var invalidPriceJson = "[{\"price\":0.100,\"percentage\":50}]";
        var cmd2 = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "XANUSDT",
            AssetName: "XAN / USDT",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 1000m,
            MultipleTakeProfitsJson: invalidPriceJson);

        var result2 = await handler.HandleAsync(cmd2, CancellationToken.None);
        result2.IsSuccess.Should().BeFalse();
        result2.Error.Should().Contain("must be above entry price");

        // Case 3: Valid multi-level TP configuration
        var validJson = "[{\"price\":0.112,\"percentage\":50},{\"price\":0.120,\"percentage\":50}]";
        var cmd3 = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "XANUSDT",
            AssetName: "XAN / USDT",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 1000m,
            MultipleTakeProfitsJson: validJson);

        var result3 = await handler.HandleAsync(cmd3, CancellationToken.None);
        result3.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ModifyTradingPosition_Long_ValidStops_UpdatesSuccessfully()
    {
        // Arrange
        var position = TradingPosition.Open(
            accountId: Guid.NewGuid(),
            userId: _userId,
            tenantId: _tenantId,
            symbol: "XANUSDT",
            assetName: "XAN / USDT",
            assetClass: AssetClass.Crypto,
            side: TradingOrderSide.Buy,
            quantity: 1000m,
            entryPrice: 0.108m,
            leverage: 10,
            stopLoss: 0.105m,
            takeProfit: 0.115m);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.110m);

        var handler = new ModifyTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            StopLoss: 0.106m,
            TakeProfit: 0.120m);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.StopLoss.Should().Be(0.106m);
        position.TakeProfit.Should().Be(0.120m);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ModifyTradingPosition_Long_InvalidStopLossAboveMarket_ReturnsFailure()
    {
        // Arrange
        var position = TradingPosition.Open(
            accountId: Guid.NewGuid(),
            userId: _userId,
            tenantId: _tenantId,
            symbol: "XANUSDT",
            assetName: "XAN / USDT",
            assetClass: AssetClass.Crypto,
            side: TradingOrderSide.Buy,
            quantity: 1000m,
            entryPrice: 0.108m,
            leverage: 10);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.110m);

        var handler = new ModifyTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        // Attempting to set SL higher than current price (0.112 > 0.110)
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            StopLoss: 0.112m);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must be strictly below current market price");
    }

    [Fact]
    public async Task ModifyTradingPosition_Short_AllowsClearingStops()
    {
        // Arrange: Short position with initial SL and TP
        var position = TradingPosition.Open(
            accountId: Guid.NewGuid(),
            userId: _userId,
            tenantId: _tenantId,
            symbol: "XANUSDT",
            assetName: "XAN / USDT",
            assetClass: AssetClass.Crypto,
            side: TradingOrderSide.Sell,
            quantity: 1000m,
            entryPrice: 0.108m,
            leverage: 10,
            stopLoss: 0.115m,
            takeProfit: 0.100m);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.105m);

        var handler = new ModifyTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        // Passing 0 or null removes the stops
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            StopLoss: 0m,
            TakeProfit: null);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.StopLoss.Should().BeNull();
        position.TakeProfit.Should().BeNull();
    }
}
