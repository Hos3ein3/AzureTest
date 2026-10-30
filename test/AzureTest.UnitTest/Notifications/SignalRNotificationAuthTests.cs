using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.MVC.Hubs;
using AzureTest.MVC.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Notifications;

public class SignalRNotificationAuthTests
{
    [Fact]
    public async Task NotificationHub_WithJwtBearerSubClaim_ShouldJoinUserGuidGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockTracker = new Mock<IUserTracker>();
        var hub = new NotificationHub(mockTracker.Object);
        var mockContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();
        var mockClients = new Mock<IHubCallerClients>();
        var mockAll = new Mock<IClientProxy>();

        mockClients.Setup(c => c.All).Returns(mockAll.Object);

        // JWT token claims with "sub"
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Email, "trader@azuretest.io")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        mockContext.Setup(c => c.User).Returns(principal);
        mockContext.Setup(c => c.ConnectionId).Returns("conn-jwt-1");

        hub.Context = mockContext.Object;
        hub.Groups = mockGroups.Object;
        hub.Clients = mockClients.Object;

        // Act
        await hub.OnConnectedAsync();

        // Assert
        var expectedGroup = $"user_{userId.ToString().ToLowerInvariant()}";
        mockGroups.Verify(g => g.AddToGroupAsync("conn-jwt-1", expectedGroup, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotificationHub_WithCookieIdentityNameIdentifier_ShouldJoinUserGuidGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockTracker = new Mock<IUserTracker>();
        var hub = new NotificationHub(mockTracker.Object);
        var mockContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();
        var mockClients = new Mock<IHubCallerClients>();
        var mockAll = new Mock<IClientProxy>();

        mockClients.Setup(c => c.All).Returns(mockAll.Object);

        // Cookie session claims with NameIdentifier
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@azuretest.io")
        };
        var identity = new ClaimsIdentity(claims, "Identity.Application");
        var principal = new ClaimsPrincipal(identity);

        mockContext.Setup(c => c.User).Returns(principal);
        mockContext.Setup(c => c.ConnectionId).Returns("conn-cookie-1");

        hub.Context = mockContext.Object;
        hub.Groups = mockGroups.Object;
        hub.Clients = mockClients.Object;

        // Act
        await hub.OnConnectedAsync();

        // Assert
        var expectedGroup = $"user_{userId.ToString().ToLowerInvariant()}";
        mockGroups.Verify(g => g.AddToGroupAsync("conn-cookie-1", expectedGroup, It.IsAny<CancellationToken>()), Times.Once);
        mockTracker.Verify(t => t.UserConnected("conn-cookie-1", userId.ToString(), "testuser"), Times.Once);
    }

    [Fact]
    public async Task NotificationDispatcher_ShouldBroadcastToMatchingUserGuidGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockGroup = new Mock<IClientProxy>();

        var expectedGroup = $"user_{userId.ToString().ToLowerInvariant()}";
        mockClients.Setup(c => c.Group(expectedGroup)).Returns(mockGroup.Object);
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var dispatcher = new NotificationHubDispatcher(mockHubContext.Object);
        var notification = new NotificationDto(
            Guid.NewGuid(),
            userId,
            "Trade Order",
            "Order filled",
            "Trade",
            null,
            false,
            null,
            DateTime.UtcNow,
            "Just now",
            null);

        // Act
        await dispatcher.SendToUserAsync(userId, notification);

        // Assert
        mockClients.Verify(c => c.Group(expectedGroup), Times.Once);
        mockGroup.Verify(g => g.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1 && notification.Equals(args[0])), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinUserGroup_ShouldAddConnectionToSpecifiedGroup()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var mockTracker = new Mock<IUserTracker>();
        var hub = new NotificationHub(mockTracker.Object);
        var mockContext = new Mock<HubCallerContext>();
        var mockGroups = new Mock<IGroupManager>();

        mockContext.Setup(c => c.ConnectionId).Returns("conn-reconnect-1");
        hub.Context = mockContext.Object;
        hub.Groups = mockGroups.Object;

        // Act
        await hub.JoinUserGroup(userId);

        // Assert
        var expectedGroup = $"user_{userId.ToLowerInvariant()}";
        mockGroups.Verify(g => g.AddToGroupAsync("conn-reconnect-1", expectedGroup, It.IsAny<CancellationToken>()), Times.Once);
    }
}
