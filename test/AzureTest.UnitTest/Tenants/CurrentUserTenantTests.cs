using System;
using System.Security.Claims;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Tenants;

public class CurrentUserTenantTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;

    public CurrentUserTenantTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _tenantProviderMock = new Mock<ITenantProvider>();
    }

    [Fact]
    public void CurrentUser_TenantId_WhenMultiTenancyEnabled_ShouldDelegateTo_TenantProvider()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        _tenantProviderMock.Setup(x => x.IsMultiTenancyEnabled).Returns(true);
        _tenantProviderMock.Setup(x => x.CurrentTenantId).Returns(expectedTenantId);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object, _serviceProviderMock.Object, _tenantProviderMock.Object);

        // Act & Assert
        currentUser.TenantId.Should().Be(expectedTenantId);
    }

    [Fact]
    public void CurrentUser_TenantId_WhenMultiTenancyDisabled_ShouldFallbackTo_UserId()
    {
        // Arrange
        var realUserId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, realUserId.ToString())
        }, "TestAuth"));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _tenantProviderMock.Setup(x => x.IsMultiTenancyEnabled).Returns(false);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object, _serviceProviderMock.Object, _tenantProviderMock.Object);

        // Act & Assert
        currentUser.TenantId.Should().Be(realUserId);
    }

    [Fact]
    public void BelongToCurrentUser_WhenMultiTenancyActive_ShouldCompareAgainst_CurrentTenantId()
    {
        // Arrange
        var realUserId = Guid.NewGuid();
        var impersonatedTenantId = Guid.NewGuid();

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, realUserId.ToString())
        }, "TestAuth"));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _tenantProviderMock.Setup(x => x.IsMultiTenancyEnabled).Returns(true);
        _tenantProviderMock.Setup(x => x.CurrentTenantId).Returns(impersonatedTenantId);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object, _serviceProviderMock.Object, _tenantProviderMock.Object);

        // Act & Assert: Should match the effective tenant ID, NOT the real user ID
        currentUser.BelongToCurrentUser(impersonatedTenantId).Should().BeTrue();
        currentUser.BelongToCurrentUser(realUserId).Should().BeFalse();
    }

    [Fact]
    public void BelongToCurrentUser_WhenMultiTenancyDisabled_ShouldCompareAgainst_RealUserId()
    {
        // Arrange
        var realUserId = Guid.NewGuid();

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, realUserId.ToString())
        }, "TestAuth"));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _tenantProviderMock.Setup(x => x.IsMultiTenancyEnabled).Returns(false);
        _tenantProviderMock.Setup(x => x.CurrentTenantId).Returns(Guid.Empty);

        var currentUser = new CurrentUser(_httpContextAccessorMock.Object, _serviceProviderMock.Object, _tenantProviderMock.Object);

        // Act & Assert
        currentUser.BelongToCurrentUser(realUserId).Should().BeTrue();
        currentUser.BelongToCurrentUser(Guid.NewGuid()).Should().BeFalse();
    }
}
