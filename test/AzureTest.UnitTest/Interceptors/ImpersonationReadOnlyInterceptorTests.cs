using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Interceptors;

public class ImpersonationReadOnlyInterceptorTests
{
    private readonly Mock<ITenantProvider> _tenantProviderMock;

    public ImpersonationReadOnlyInterceptorTests()
    {
        _tenantProviderMock = new Mock<ITenantProvider>();
    }

    private AzureTestDbContext CreateDbContext(IConfiguration? configuration = null)
    {
        var interceptor = new ImpersonationReadOnlyInterceptor(_tenantProviderMock.Object, configuration);

        var options = new DbContextOptionsBuilder<AzureTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new AzureTestDbContext(options);
    }

    [Fact]
    public async Task When_AdminIsNotImpersonating_AddingTenantEntity_ShouldSucceed()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_AddingTenantEntity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);

        // Act & Assert
        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ImpersonationReadOnlyInterceptor.ReadOnlyErrorMessage);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_ModifyingTenantEntity_ShouldThrowInvalidOperationException()
    {
        // Arrange: Insert first without impersonation
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);
        await context.SaveChangesAsync();

        // Now admin impersonates
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        debt.Amount = 200m;

        // Act & Assert
        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ImpersonationReadOnlyInterceptor.ReadOnlyErrorMessage);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_DeletingTenantEntity_ShouldThrowInvalidOperationException()
    {
        // Arrange: Insert first without impersonation
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);
        await context.SaveChangesAsync();

        // Now admin impersonates
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        context.Debts.Remove(debt);

        // Act & Assert
        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ImpersonationReadOnlyInterceptor.ReadOnlyErrorMessage);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_And_AllowImpersonationWritesIsTrue_ViaTenantProvider_AddingTenantEntity_ShouldSucceed()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.AllowImpersonationWrites).Returns(true);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 150m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_And_AllowImpersonationWritesIsTrue_ViaTenantProvider_ModifyingTenantEntity_ShouldSucceed()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.AllowImpersonationWrites).Returns(true);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);
        await context.SaveChangesAsync();

        debt.Amount = 250m;

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        debt.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_And_AllowImpersonationWritesIsTrue_ViaTenantProvider_DeletingTenantEntity_ShouldSucceed()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.AllowImpersonationWrites).Returns(true);
        using var context = CreateDbContext();

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);
        await context.SaveChangesAsync();

        context.Debts.Remove(debt);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_AdminIsImpersonating_And_AllowImpersonationWritesIsTrue_ViaConfiguration_AddingTenantEntity_ShouldSucceed()
    {
        // Arrange
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.AllowImpersonationWrites).Returns(false);

        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "FeatureManagement:AllowImpersonationWrites", "true" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        using var context = CreateDbContext(configuration);

        var debt = new Debt
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 300m,
            DateOfBorrow = DateTime.UtcNow,
            DebtPersonId = Guid.NewGuid()
        };
        context.Debts.Add(debt);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }
}
