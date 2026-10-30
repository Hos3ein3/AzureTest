using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.Domain.Documents.Notifications;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Infrastructure.Jobs;
using AzureTest.Infrastructure.Services;
using AzureTest.UnitTest.Trading;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Notifications;

public class NotificationFeatureTests
{
    private readonly Mock<INoSqlUnitOfWork> _noSqlUowMock;
    private readonly Mock<INotificationHistoryRepository> _notificationRepoMock;
    private readonly Mock<IPushSubscriptionRepository> _pushRepoMock;
    private readonly Mock<IWebPushService> _webPushMock;
    private readonly Mock<INotificationHubDispatcher> _hubDispatcherMock;
    private readonly Mock<ILogger<AppNotificationService>> _loggerMock;

    public NotificationFeatureTests()
    {
        _noSqlUowMock = new Mock<INoSqlUnitOfWork>();
        _notificationRepoMock = new Mock<INotificationHistoryRepository>();
        _pushRepoMock = new Mock<IPushSubscriptionRepository>();
        _webPushMock = new Mock<IWebPushService>();
        _hubDispatcherMock = new Mock<INotificationHubDispatcher>();
        _loggerMock = new Mock<ILogger<AppNotificationService>>();

        _noSqlUowMock.Setup(u => u.NotificationHistories).Returns(_notificationRepoMock.Object);
        _noSqlUowMock.Setup(u => u.PushSubscriptions).Returns(_pushRepoMock.Object);
    }

    [Fact]
    public void NotificationHistoryDocument_CreationAndMarkAsRead_ShouldUpdateStateCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var doc = NotificationHistoryDocument.Create(
            userId,
            tenantId,
            "Trade Executed",
            "Bought 1.5 BTC",
            "Trade",
            "/Trading");

        // Assert
        doc.UserId.Should().Be(userId);
        doc.TenantId.Should().Be(tenantId);
        doc.Title.Should().Be("Trade Executed");
        doc.IsRead.Should().BeFalse();
        doc.ReadAt.Should().BeNull();

        // Mark as read
        doc.MarkAsRead();
        doc.IsRead.Should().BeTrue();
        doc.ReadAt.Should().NotBeNull();

        // Mark as unread
        doc.MarkAsUnread();
        doc.IsRead.Should().BeFalse();
        doc.ReadAt.Should().BeNull();
    }

    [Fact]
    public async Task GetUserNotificationsQueryHandler_ShouldReturnMappedDtosForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var docs = new List<NotificationHistoryDocument>
        {
            NotificationHistoryDocument.Create(userId, Guid.Empty, "Alert 1", "Message 1"),
            NotificationHistoryDocument.Create(userId, Guid.Empty, "Alert 2", "Message 2")
        };

        _notificationRepoMock.Setup(r => r.GetByUserAsync(userId, null, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs);

        var handler = new GetUserNotificationsQueryHandler(_noSqlUowMock.Object);
        var query = new GetUserNotificationsQuery(userId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Title.Should().Be("Alert 1");
        result.Value[1].Title.Should().Be("Alert 2");
    }

    [Fact]
    public async Task MarkNotificationAsReadCommandHandler_WhenSingleNotification_ShouldMarkItRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doc = NotificationHistoryDocument.Create(userId, Guid.Empty, "Alert", "Msg");
        _notificationRepoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var handler = new MarkNotificationAsReadCommandHandler(_noSqlUowMock.Object);
        var command = new MarkNotificationAsReadCommand(userId, doc.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        doc.IsRead.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.UpdateAsync(doc.Id, doc, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsReadCommandHandler_WhenNoId_ShouldMarkAllUnreadAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doc1 = NotificationHistoryDocument.Create(userId, Guid.Empty, "Alert 1", "Msg 1");
        var doc2 = NotificationHistoryDocument.Create(userId, Guid.Empty, "Alert 2", "Msg 2");

        _notificationRepoMock.Setup(r => r.GetByUserAsync(userId, true, 200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationHistoryDocument> { doc1, doc2 });

        var handler = new MarkNotificationAsReadCommandHandler(_noSqlUowMock.Object);
        var command = new MarkNotificationAsReadCommand(userId, null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        doc1.IsRead.Should().BeTrue();
        doc2.IsRead.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<NotificationHistoryDocument>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AppNotificationService_ShouldPersistDocumentAndBroadcastSignalRAndWebPush()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var service = new AppNotificationService(
            _noSqlUowMock.Object,
            _webPushMock.Object,
            _hubDispatcherMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SendNotificationAsync(
            userId,
            tenantId,
            "Trade Opened",
            "Bought 1 BTC @ 50,000",
            "Trade",
            "/Trading");

        // Assert
        result.IsSuccess.Should().BeTrue();

        _notificationRepoMock.Verify(r => r.AddAsync(
            It.Is<NotificationHistoryDocument>(d => d.UserId == userId && d.Title == "Trade Opened"),
            It.IsAny<CancellationToken>()), Times.Once);

        _hubDispatcherMock.Verify(h => h.SendToUserAsync(
            userId,
            It.Is<NotificationDto>(n => n.Title == "Trade Opened"),
            It.IsAny<CancellationToken>()), Times.Once);

        _webPushMock.Verify(w => w.SendPushAsync(
            userId,
            "Trade Opened",
            "Bought 1 BTC @ 50,000",
            "/Trading",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanupReadNotificationsJob_ShouldPurgeReadNotificationsOlderThan10Days()
    {
        // Arrange
        _notificationRepoMock.Setup(r => r.DeleteReadOlderThanAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var logger = new Mock<ILogger<CleanupReadNotificationsJob>>();
        var job = new CleanupReadNotificationsJob(_noSqlUowMock.Object, logger.Object);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        _notificationRepoMock.Verify(r => r.DeleteReadOlderThanAsync(
            It.Is<DateTime>(dt => dt <= DateTime.UtcNow.AddDays(-9.99)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceTradingOrder_WhenMarketOrderFills_ShouldSendNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var account = TradingAccount.Create(userId, tenantId, "Test Account", "USD", 50000m);

        var uowMock = new Mock<IUnitOfWork>();
        var accountRepoMock = new Mock<AzureTest.Application.Interfaces.Data.Trading.ITradingAccountRepository>();
        var positionRepoMock = new Mock<AzureTest.Application.Interfaces.Data.Trading.ITradingPositionRepository>();
        var orderRepoMock = new Mock<AzureTest.Application.Interfaces.Data.Trading.ITradingOrderRepository>();
        var marketDataMock = new Mock<ITradingMarketDataService>();
        var notificationServiceMock = new Mock<IAppNotificationService>();

        uowMock.Setup(u => u.TradingAccounts).Returns(accountRepoMock.Object);
        uowMock.Setup(u => u.TradingPositions).Returns(positionRepoMock.Object);
        uowMock.Setup(u => u.TradingOrders).Returns(orderRepoMock.Object);

        accountRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TradingAccount> { account });
        marketDataMock.Setup(m => m.GetCurrentPriceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(65000m);

        var handler = new PlaceTradingOrderCommandHandler(uowMock.Object, marketDataMock.Object, notificationServiceMock.Object);
        var command = new PlaceTradingOrderCommand(
            userId,
            tenantId,
            "BTCUSDT",
            "Bitcoin",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            TradingOrderType.Market,
            0.1m,
            65000m);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notificationServiceMock.Verify(n => n.SendNotificationAsync(
            userId,
            tenantId,
            It.Is<string>(s => s.Contains("Paper Trade Opened")),
            It.Is<string>(s => s.Contains("BTCUSDT")),
            "Trade",
            "/Trading",
            It.IsAny<Dictionary<string, string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseTradingPosition_WhenPositionCloses_ShouldSendNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var account = TradingAccount.Create(userId, tenantId, "Test Account", "USD", 50000m);
        var position = TradingPosition.Open(
            account.Id,
            userId,
            tenantId,
            "ETHUSDT",
            "Ethereum",
            AssetClass.Crypto,
            TradingOrderSide.Buy,
            1.0m,
            3000m,
            1);

        var uowMock = new Mock<IUnitOfWork>();
        var accountRepoMock = new Mock<AzureTest.Application.Interfaces.Data.Trading.ITradingAccountRepository>();
        var positionRepoMock = new Mock<AzureTest.Application.Interfaces.Data.Trading.ITradingPositionRepository>();
        var marketDataMock = new Mock<ITradingMarketDataService>();
        var notificationServiceMock = new Mock<IAppNotificationService>();

        uowMock.Setup(u => u.TradingAccounts).Returns(accountRepoMock.Object);
        uowMock.Setup(u => u.TradingPositions).Returns(positionRepoMock.Object);

        positionRepoMock.Setup(r => r.GetByIdAsync(position.Id)).ReturnsAsync(position);
        accountRepoMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        marketDataMock.Setup(m => m.GetCurrentPriceAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3200m);

        var handler = new CloseTradingPositionCommandHandler(uowMock.Object, marketDataMock.Object, TestEventDispatcherFactory.Create(uowMock.Object));
        var command = new CloseTradingPositionCommand(position.Id, userId, tenantId, 3200m);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notificationServiceMock.Verify(n => n.SendNotificationAsync(
            userId,
            tenantId,
            It.Is<string>(s => s.Contains("Paper Trade Closed")),
            It.Is<string>(s => s.Contains("ETHUSDT") && s.Contains("PnL")),
            "Trade",
            "/Trading",
            It.IsAny<Dictionary<string, string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
