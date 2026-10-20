using System;
using System.Collections.Generic;
using System.Security.Claims;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Tenants;

public class TenantProviderTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly InfrastructureFeatureOptions _options;

    public TenantProviderTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _options = new InfrastructureFeatureOptions
        {
            EnableMultiTenancy = true
        };
    }

    private TenantProvider CreateSut(InfrastructureFeatureOptions? options = null)
    {
        var opt = Options.Create(options ?? _options);
        return new TenantProvider(_httpContextAccessorMock.Object, opt);
    }

    [Fact]
    public void When_MultiTenancyIsDisabled_CurrentTenantId_ShouldReturn_GuidEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateHttpContext(userId, isAdmin: true, cookieTenantId: Guid.NewGuid());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var disabledOptions = new InfrastructureFeatureOptions { EnableMultiTenancy = false };
        var sut = CreateSut(disabledOptions);

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(Guid.Empty);
        sut.IsMultiTenancyEnabled.Should().BeFalse();
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    [Fact]
    public void When_MultiTenancyIsEnabled_And_RegularUser_CurrentTenantId_ShouldReturn_RealUserId()
    {
        // Arrange: A normal user attempts to supply an impersonation cookie
        var realUserId = Guid.NewGuid();
        var attemptedImpersonationId = Guid.NewGuid();

        var context = CreateHttpContext(realUserId, isAdmin: false, cookieTenantId: attemptedImpersonationId);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert: Security isolation strictly forces RealUserId and ignores cookie
        result.Should().Be(realUserId);
        sut.RealUserId.Should().Be(realUserId);
        sut.IsAdmin.Should().BeFalse();
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    [Fact]
    public void When_MultiTenancyIsEnabled_And_AdminWithoutImpersonation_CurrentTenantId_ShouldReturn_AdminId()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var context = CreateHttpContext(adminId, isAdmin: true);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(adminId);
        sut.RealUserId.Should().Be(adminId);
        sut.IsAdmin.Should().BeTrue();
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    [Fact]
    public void When_MultiTenancyIsEnabled_And_AdminWithValidTenantCookie_CurrentTenantId_ShouldReturn_TargetTenantId()
    {
        // Arrange: Admin impersonates another tenant via cookie
        var adminId = Guid.NewGuid();
        var targetTenantId = Guid.NewGuid();

        var context = CreateHttpContext(adminId, isAdmin: true, cookieTenantId: targetTenantId);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(targetTenantId);
        sut.RealUserId.Should().Be(adminId);
        sut.IsAdminImpersonating.Should().BeTrue();
    }

    [Fact]
    public void When_MultiTenancyIsEnabled_And_AdminWithValidTenantHeader_CurrentTenantId_ShouldReturn_HeaderTenantId()
    {
        // Arrange: Admin impersonates another tenant via X-Tenant-Id header
        var adminId = Guid.NewGuid();
        var headerTenantId = Guid.NewGuid();

        var context = CreateHttpContext(adminId, isAdmin: true, headerTenantId: headerTenantId);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(headerTenantId);
        sut.IsAdminImpersonating.Should().BeTrue();
    }

    [Fact]
    public void When_AdminCookie_Targets_AdminOwnUserId_IsAdminImpersonating_ShouldBeFalse()
    {
        // Arrange: Admin sets cookie targeting their own user ID
        var adminId = Guid.NewGuid();
        var context = CreateHttpContext(adminId, isAdmin: true, cookieTenantId: adminId);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(adminId);
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    [Fact]
    public void When_AdminCookie_HasInvalidGuid_CurrentTenantId_ShouldFallbackTo_RealUserId()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adminId.ToString()),
            new(ClaimTypes.Role, "Admin")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        context.Request.Headers["Cookie"] = $"{TenantProvider.AdminTenantCookieKey}=invalid-guid-value";

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(adminId);
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    [Fact]
    public void When_UnauthenticatedUser_CurrentTenantId_ShouldReturn_GuidEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext(); // No User claims
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var sut = CreateSut();

        // Act
        var result = sut.CurrentTenantId;

        // Assert
        result.Should().Be(Guid.Empty);
        sut.RealUserId.Should().Be(Guid.Empty);
        sut.IsAdmin.Should().BeFalse();
        sut.IsAdminImpersonating.Should().BeFalse();
    }

    private static HttpContext CreateHttpContext(
        Guid userId,
        bool isAdmin,
        Guid? cookieTenantId = null,
        Guid? headerTenantId = null)
    {
        var context = new DefaultHttpContext();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "Test User")
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        if (cookieTenantId.HasValue)
        {
            context.Request.Headers["Cookie"] = $"{TenantProvider.AdminTenantCookieKey}={cookieTenantId.Value}";
        }

        if (headerTenantId.HasValue)
        {
            context.Request.Headers[TenantProvider.TenantHeaderKey] = headerTenantId.Value.ToString();
        }

        return context;
    }
}
