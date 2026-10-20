using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents.Audit;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.AuditLogs;

public class AuditLogServiceTests
{
    private readonly Mock<INoSqlUnitOfWork> _noSqlUnitOfWorkMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;

    public AuditLogServiceTests()
    {
        _noSqlUnitOfWorkMock = new Mock<INoSqlUnitOfWork>();
        _auditLogRepoMock = new Mock<IAuditLogRepository>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _currentUserMock = new Mock<ICurrentUser>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<AuditLogService>>();

        _noSqlUnitOfWorkMock.Setup(u => u.AuditLogs).Returns(_auditLogRepoMock.Object);
    }

    private AuditLogService CreateService(bool enableImmutableAuditLog = true)
    {
        var options = Options.Create(new InfrastructureFeatureOptions
        {
            ImmutableAuditLog = enableImmutableAuditLog
        });

        return new AuditLogService(
            _noSqlUnitOfWorkMock.Object,
            _tenantProviderMock.Object,
            _currentUserMock.Object,
            _httpContextAccessorMock.Object,
            options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task When_FeatureDisabled_LogAsync_ShouldNotRecordLog()
    {
        // Arrange
        var service = CreateService(enableImmutableAuditLog: false);

        // Act
        await service.LogAsync("Command", "CreateDebtCommand", new { Amount = 100 }, 12, true);

        // Assert
        _auditLogRepoMock.Verify(r => r.AddAsync(It.IsAny<AuditLogDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When_FeatureEnabled_LogCommandAsync_ShouldRecordAuditDocument()
    {
        // Arrange
        var service = CreateService(enableImmutableAuditLog: true);
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.Id).Returns(userId);
        _currentUserMock.Setup(u => u.FullName).Returns("Alice Smith");
        _currentUserMock.Setup(u => u.Email).Returns("alice@example.com");
        _tenantProviderMock.Setup(t => t.CurrentTenantId).Returns(tenantId);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);

        AuditLogDocument? captured = null;
        _auditLogRepoMock.Setup(r => r.AddAsync(It.IsAny<AuditLogDocument>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogDocument, CancellationToken>((doc, _) => captured = doc)
            .Returns(Task.CompletedTask);

        // Act
        await service.LogCommandAsync(new { Name = "TestCommand", password = "SecretPassword123" }, 45, true);

        // Assert
        _auditLogRepoMock.Verify(r => r.AddAsync(It.IsAny<AuditLogDocument>(), It.IsAny<CancellationToken>()), Times.Once);
        captured.Should().NotBeNull();
        captured!.ActionType.Should().Be("Command");
        captured.UserId.Should().Be(userId);
        captured.TenantId.Should().Be(tenantId);
        captured.UserName.Should().Be("Alice Smith");
        captured.UserEmail.Should().Be("alice@example.com");
        captured.ExecutionDurationMs.Should().Be(45);
        captured.IsSuccess.Should().BeTrue();
        captured.Payload.Should().NotContain("SecretPassword123");
        captured.Payload.Should().Contain("***");
    }

    [Fact]
    public async Task When_AdminImpersonating_ShouldRecordImpersonatedByAdminId()
    {
        // Arrange
        var service = CreateService(enableImmutableAuditLog: true);
        var targetTenantId = Guid.NewGuid();
        var realAdminId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.Id).Returns(realAdminId);
        _tenantProviderMock.Setup(t => t.CurrentTenantId).Returns(targetTenantId);
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(true);
        _tenantProviderMock.Setup(t => t.RealUserId).Returns(realAdminId);

        AuditLogDocument? captured = null;
        _auditLogRepoMock.Setup(r => r.AddAsync(It.IsAny<AuditLogDocument>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogDocument, CancellationToken>((doc, _) => captured = doc)
            .Returns(Task.CompletedTask);

        // Act
        await service.LogCommandAsync(new { Action = "TenantImpersonationAction" }, 15, true);

        // Assert
        captured.Should().NotBeNull();
        captured!.ImpersonatedByAdminId.Should().Be(realAdminId);
        captured.TenantId.Should().Be(targetTenantId);
    }

    [Fact]
    public async Task When_RegularUser_GetLogsAsync_ShouldFilterByTheirOwnUserId()
    {
        // Arrange
        var service = CreateService(enableImmutableAuditLog: true);
        var regularUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _auditLogRepoMock.Setup(r => r.GetFilteredLogsAsync(
                It.Is<Guid?>(t => t == tenantId),
                It.Is<Guid?>(u => u == regularUserId),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLogDocument>());

        _auditLogRepoMock.Setup(r => r.CountFilteredLogsAsync(
                It.Is<Guid?>(t => t == tenantId),
                It.Is<Guid?>(u => u == regularUserId),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        // Act
        var result = await service.GetLogsAsync(
            isAdmin: false,
            currentUserId: regularUserId,
            currentTenantId: tenantId);

        // Assert: Verified that the repository was called with the user's specific Id
        _auditLogRepoMock.Verify(r => r.GetFilteredLogsAsync(
            tenantId,
            regularUserId,
            null,
            null,
            1,
            50,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task When_AdminUser_GetLogsAsync_ShouldNotRestrictToSingleUserId()
    {
        // Arrange
        var service = CreateService(enableImmutableAuditLog: true);
        var adminId = Guid.NewGuid();
        _tenantProviderMock.Setup(t => t.IsAdminImpersonating).Returns(false);

        _auditLogRepoMock.Setup(r => r.GetFilteredLogsAsync(
                null,
                null,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLogDocument>());

        // Act
        var result = await service.GetLogsAsync(
            isAdmin: true,
            currentUserId: adminId,
            currentTenantId: adminId);

        // Assert: Admin with no active impersonation queries all logs globally
        _auditLogRepoMock.Verify(r => r.GetFilteredLogsAsync(
            null,
            null,
            null,
            null,
            1,
            50,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
