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

    [Fact]
    public async Task ModifyTradingPosition_WithMultipleTakeProfits_Valid_UpdatesPositionAndJson()
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
            stopLoss: 0.106m,
            takeProfit: 0.115m);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.110m);

        var handler = new ModifyTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var multiTpJson = "[{\"price\":0.115,\"percentage\":30},{\"price\":0.120,\"percentage\":30},{\"price\":0.125,\"percentage\":40}]";
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            StopLoss: 0.106m,
            TakeProfit: null,
            MultipleTakeProfitsJson: multiTpJson);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.TakeProfit.Should().Be(0.115m);
        position.MultipleTakeProfitsJson.Should().Be(multiTpJson);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ModifyTradingPosition_WithMultipleTakeProfits_InvalidPercentageSum_ReturnsFailure()
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
        // Sum is 80%, not 100%
        var multiTpJson = "[{\"price\":0.115,\"percentage\":40},{\"price\":0.120,\"percentage\":40}]";
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            MultipleTakeProfitsJson: multiTpJson);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must equal 100%");
    }

    [Fact]
    public async Task ModifyTradingPosition_WithMultipleTakeProfits_InvalidPriceBelowMarket_ReturnsFailure()
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
        // Level 1 (0.105) is below current market price (0.110) for Buy position
        var multiTpJson = "[{\"price\":0.105,\"percentage\":50},{\"price\":0.120,\"percentage\":50}]";
        var cmd = new ModifyTradingPositionCommand(
            PositionId: position.Id,
            UserId: _userId,
            TenantId: _tenantId,
            MultipleTakeProfitsJson: multiTpJson);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must be above current price");
    }

    [Fact]
    public void TradingNotificationFormatter_FormatsMicroPnlAndPrices_AccuratelyWithoutZeroRounding()
    {
        // Arrange & Act
        var formattedPnlSmallPositive = TradingNotificationFormatter.FormatPnl(0.0025m);
        var formattedPnlSmallNegative = TradingNotificationFormatter.FormatPnl(-0.0040m);
        var formattedPnlZero = TradingNotificationFormatter.FormatPnl(0m);
        var formattedPnlStandard = TradingNotificationFormatter.FormatPnl(15.50m);
        var formattedPriceSmall = TradingNotificationFormatter.FormatPrice(0.0108m);
        var formattedPriceMicro = TradingNotificationFormatter.FormatPrice(0.00000015m);

        // Assert
        formattedPnlSmallPositive.Should().Be("+$0.00250");
        formattedPnlSmallPositive.Should().NotBe("+$0.00");
        formattedPnlSmallNegative.Should().Be("-$0.00400");
        formattedPnlSmallNegative.Should().NotBe("-$0.00");
        formattedPnlZero.Should().Be("$0.00000");
        formattedPnlStandard.Should().Be("+$15.50000");
        formattedPriceSmall.Should().Be("$0.01080");
        formattedPriceSmall.Should().NotBe("$0.01");
        formattedPriceMicro.Should().Be("$0.00000015");
    }

    [Fact]
    public async Task CloseTradingPosition_WithMicroPnL_SendsNotificationWithAccuratePnLAndMetadata()
    {
        // Arrange: Position with small quantity where PnL is 0.002
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 1000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 10m,
            entryPrice: 0.108m,
            leverage: 10);

        account.DeductMargin(position.InitialMargin);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        var mockNotification = new Mock<AzureTest.Application.Interfaces.Services.IAppNotificationService>();
        string capturedMessage = string.Empty;
        Dictionary<string, string>? capturedMetadata = null;

        mockNotification
            .Setup(n => n.SendNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<Dictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, Guid, string, string, string, string?, Dictionary<string, string>?, CancellationToken>(
                (uid, tid, title, msg, type, url, meta, ct) =>
                {
                    capturedMessage = msg;
                    capturedMetadata = meta;
                })
            .ReturnsAsync(AzureTest.Application.Abstractions.Result<Guid>.Success(Guid.NewGuid()));

        var handler = new CloseTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object, mockNotification.Object);
        // Closed at 0.1082: PnL = (0.1082 - 0.1080) * 10 = +0.002
        var cmd = new CloseTradingPositionCommand(position.Id, _userId, _tenantId, 0.1082m);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.RealizedPnl.Should().Be(0.002m);

        capturedMessage.Should().NotBeNullOrEmpty();
        capturedMessage.Should().Contain("+$0.00200");
        capturedMessage.Should().NotContain("+$0.00.");

        capturedMetadata.Should().NotBeNull();
        decimal.Parse(capturedMetadata!["realizedPnl"], System.Globalization.CultureInfo.InvariantCulture).Should().Be(0.002m);
        decimal.Parse(capturedMetadata["exitPrice"], System.Globalization.CultureInfo.InvariantCulture).Should().Be(0.1082m);
    }

    [Fact]
    public async Task CloseTradingPosition_WithoutExplicitClosePrice_FetchesLiveMarketPrice_AndCalculatesNonZeroPnL()
    {
        // Arrange
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 1000m);
        var position = TradingPosition.Open(
            account.Id,
            _userId,
            _tenantId,
            "XANUSDT",
            "XAN / USDT",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            quantity: 1000m,
            entryPrice: 0.108m,
            leverage: 10);

        account.DeductMargin(position.InitialMargin);

        _mockPositions.Setup(p => p.GetByIdAsync(position.Id)).ReturnsAsync(position);
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        // Market data returns live price 0.115 (different from entry 0.108)
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("XANUSDT", 0m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.115m);

        var handler = new CloseTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        // No explicit ClosePrice provided (simulating button click without form price)
        var cmd = new CloseTradingPositionCommand(position.Id, _userId, _tenantId, null);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        position.ClosePrice.Should().Be(0.115m);
        // PnL = (0.115 - 0.108) * 1000 = +7.00
        position.RealizedPnl.Should().Be(7m);
        account.RealizedPnl.Should().Be(7m);
        account.CashBalance.Should().Be(1007m);
    }

    [Fact]
    public async Task BtcPosition_WithNineDollarMargin_DeductsMargin_And_OnLoss_ReleasesMarginAndDecreasesCashBalance()
    {
        // Scenario reported by user: User puts $9 margin on BTC (price ~$65,000)
        // At 10x leverage, notional = $90, quantity = 90 / 65,000 = 0.001385 BTC
        var initialCash = 1000m;
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", initialCash);
        _mockAccounts.Setup(a => a.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);

        var btcPrice = 65000m;
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("BTCUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(btcPrice);

        TradingPosition? openedPosition = null;
        _mockPositions.Setup(p => p.Add(It.IsAny<TradingPosition>()))
            .Callback<TradingPosition>(p => openedPosition = p);

        var placeHandler = new PlaceTradingOrderCommandHandler(_mockUow.Object, _mockMarketData.Object);
        // User inputs $9 margin at 10x leverage => quantity is 0.001385 BTC
        var cmd = new PlaceTradingOrderCommand(
            UserId: _userId,
            TenantId: _tenantId,
            Symbol: "BTCUSDT",
            AssetName: "Bitcoin",
            AssetClass: AssetClass.Crypto,
            Side: TradingOrderSide.Buy,
            OrderType: TradingOrderType.Market,
            Quantity: 0.001385m,
            Leverage: 10);

        var placeResult = await placeHandler.HandleAsync(cmd, CancellationToken.None);
        placeResult.IsSuccess.Should().BeTrue();
        openedPosition.Should().NotBeNull();

        // Initial Margin is approximately $9.00
        var expectedMargin = (0.001385m * btcPrice) / 10m; // ~9.0025
        openedPosition!.InitialMargin.Should().BeApproximately(expectedMargin, 0.01m);

        // MarginUsed is deducted (locked)
        account.MarginUsed.Should().BeApproximately(expectedMargin, 0.01m);
        account.FreeMargin.Should().BeApproximately(initialCash - expectedMargin, 0.01m);
        account.CashBalance.Should().Be(initialCash);

        // Now user closes position with a loss (price drops from 65,000 to 60,000)
        _mockPositions.Setup(p => p.GetByIdAsync(openedPosition.Id)).ReturnsAsync(openedPosition);
        var closePrice = 60000m;
        var closeHandler = new CloseTradingPositionCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var closeCmd = new CloseTradingPositionCommand(openedPosition.Id, _userId, _tenantId, closePrice);

        var closeResult = await closeHandler.HandleAsync(closeCmd, CancellationToken.None);
        closeResult.IsSuccess.Should().BeTrue();

        // Loss = (60,000 - 65,000) * 0.001385 = -6.925
        var expectedLoss = (closePrice - btcPrice) * 0.001385m;
        openedPosition.RealizedPnl.Should().BeApproximately(expectedLoss, 0.01m);
        openedPosition.Status.Should().Be(TradingPositionStatus.Closed);

        // Account MarginUsed must be completely released (0)
        account.MarginUsed.Should().Be(0m);
        // CashBalance must decrease by the exact loss amount
        account.CashBalance.Should().BeApproximately(initialCash + expectedLoss, 0.01m);
        account.FreeMargin.Should().Be(account.CashBalance);
        account.RealizedPnl.Should().BeApproximately(expectedLoss, 0.01m);
    }

    [Fact]
    public void BtcPosition_LeverageCalculation_CorrectlyScalesNotionalAndMargin()
    {
        var entryPrice = 70000m;
        var quantity = 0.1m; // 0.1 BTC = $7,000 notional

        // 1x leverage => margin = $7,000
        var pos1x = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 1);
        pos1x.InitialMargin.Should().Be(7000m);

        // 10x leverage => margin = $700
        var pos10x = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 10);
        pos10x.InitialMargin.Should().Be(700m);

        // 50x leverage => margin = $140
        var pos50x = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 50);
        pos50x.InitialMargin.Should().Be(140m);

        // 100x leverage => margin = $70
        var pos100x = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 100);
        pos100x.InitialMargin.Should().Be(70m);
    }

    [Fact]
    public void ShortPosition_ProfitAndLoss_CalculatesCorrectlyForBothDirections()
    {
        var entryPrice = 65000m;
        var quantity = 0.01m; // notional = 650

        // Short position
        var pos = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Sell, quantity, entryPrice, leverage: 10);
        pos.InitialMargin.Should().Be(65m);

        // Case A: Price drops to 60,000 (Short makes profit!)
        pos.UpdateCurrentPrice(60000m);
        // PnL = (65,000 - 60,000) * 0.01 = +50.00
        pos.UnrealizedPnl.Should().Be(50m);

        // Case B: Price rises to 68,000 (Short takes a loss)
        pos.UpdateCurrentPrice(68000m);
        // PnL = (65,000 - 68,000) * 0.01 = -30.00
        pos.UnrealizedPnl.Should().Be(-30m);
    }

    [Fact]
    public void Position_LossIsCappedAtInitialMargin_InIsolatedMarginTrading()
    {
        var entryPrice = 50000m;
        var quantity = 0.01m; // notional = 500
        // Leverage = 50x => InitialMargin = 500 / 50 = $10.00
        var pos = TradingPosition.Open(Guid.NewGuid(), _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 50);
        pos.InitialMargin.Should().Be(10m);

        // Price crashes to 20,000 (unclamped loss would be (20,000 - 50,000) * 0.01 = -300)
        pos.Close(20000m);

        // In isolated margin trading, maximum loss is 100% of InitialMargin ($10)
        pos.RealizedPnl.Should().Be(-10m);
        pos.Status.Should().Be(TradingPositionStatus.Closed);
    }

    [Fact]
    public async Task SyncPrices_LiquidatesPosition_WhenLossReachesOrExceedsInitialMargin()
    {
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 5000m);
        var entryPrice = 60000m;
        var quantity = 0.01m; // notional = 600
        // Leverage = 20x => InitialMargin = 600 / 20 = $30.00
        var pos = TradingPosition.Open(account.Id, _userId, _tenantId, "BTCUSDT", "Bitcoin", AssetClass.Crypto, TradingOrderSide.Buy, quantity, entryPrice, leverage: 20);
        account.DeductMargin(pos.InitialMargin);

        _mockPositions.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<TradingPosition> { pos });
        _mockAccounts.Setup(a => a.GetByIdAsync(account.Id)).ReturnsAsync(account);
        _mockOrders.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<TradingOrder>());

        // Price drops to 55,000 => loss = (55,000 - 60,000) * 0.01 = -50. Loss exceeds margin ($30)!
        _mockMarketData
            .Setup(m => m.GetCurrentPriceAsync("BTCUSDT", It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(55000m);

        var handler = new SyncTradingPricesCommandHandler(_mockUow.Object, _mockMarketData.Object);
        var result = await handler.HandleAsync(new SyncTradingPricesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);

        // Position is marked Liquidated
        pos.Status.Should().Be(TradingPositionStatus.Liquidated);
        // Realized loss capped at InitialMargin ($30)
        pos.RealizedPnl.Should().Be(-30m);

        // Margin is fully released back to account
        account.MarginUsed.Should().Be(0m);
        // Account CashBalance decreased by $30 (from 5000 to 4970)
        account.CashBalance.Should().Be(4970m);
        account.FreeMargin.Should().Be(4970m);
        account.RealizedPnl.Should().Be(-30m);
    }

    [Fact]
    public void TradingAccount_Recalculate_ReconcilesMarginUsedAndEquityAccurately()
    {
        var account = TradingAccount.Create(_userId, _tenantId, "Paper Trading", "USD", 5000m);

        // Simulate 2 open positions with active margins $15 and $25, and total floating PnL +$40
        account.Recalculate(activeMarginUsed: 40m, totalUnrealizedPnl: 40m);

        account.MarginUsed.Should().Be(40m);
        account.Equity.Should().Be(5040m); // CashBalance (5000) + Unrealized (40)
        account.FreeMargin.Should().Be(5000m); // Equity (5040) - MarginUsed (40)

        // When all positions close:
        account.Recalculate(activeMarginUsed: 0m, totalUnrealizedPnl: 0m);
        account.MarginUsed.Should().Be(0m);
        account.Equity.Should().Be(5000m);
        account.FreeMargin.Should().Be(5000m);
    }
}
