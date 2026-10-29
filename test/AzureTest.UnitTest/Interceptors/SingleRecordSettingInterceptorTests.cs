using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Pomodoro;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Interceptors;
using AzureTest.Persistence.Repositories.NoSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Interceptors;

public class SingleRecordSettingInterceptorTests
{
    private readonly Mock<ITenantProvider> _tenantProviderMock;

    public SingleRecordSettingInterceptorTests()
    {
        _tenantProviderMock = new Mock<ITenantProvider>();
    }

    private AzureTestDbContext CreateDbContext()
    {
        var interceptor = new SingleRecordSettingInterceptor();

        var options = new DbContextOptionsBuilder<AzureTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new AzureTestDbContext(options, _tenantProviderMock.Object);
    }

    [Fact]
    public async Task When_NoRecordExists_InsertingSingleRecordSetting_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var setting = PomodoroSetting.CreateDefault(userId, tenantId);

        context.PomodoroSettings.Add(setting);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_RecordAlreadyExists_InsertingSecondSingleRecordSetting_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var firstSetting = PomodoroSetting.CreateDefault(userId, tenantId);
        context.PomodoroSettings.Add(firstSetting);
        await context.SaveChangesAsync();

        // Act: try inserting another record for the same tenant
        var secondSetting = PomodoroSetting.CreateDefault(userId, tenantId);
        context.PomodoroSettings.Add(secondSetting);

        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(string.Format(SingleRecordSettingInterceptor.DuplicateErrorMessage, nameof(PomodoroSetting)));
    }

    [Fact]
    public async Task When_MultipleRecordsAddedInSameBatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var first = PomodoroSetting.CreateDefault(userId, tenantId);
        var second = PomodoroSetting.CreateDefault(userId, tenantId);

        context.PomodoroSettings.AddRange(first, second);

        // Act
        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(string.Format(SingleRecordSettingInterceptor.DuplicateErrorMessage, nameof(PomodoroSetting)));
    }

    [Fact]
    public async Task When_UpdatingExistingSingleRecordSetting_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var setting = PomodoroSetting.CreateDefault(userId, tenantId);
        context.PomodoroSettings.Add(setting);
        await context.SaveChangesAsync();

        // Act: update properties on the existing setting
        setting.Update(30, 5, 20, 4, true, false, true, AzureTest.Domain.Enums.Pomodoro.TimerSkin.Digital);
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_DifferentTenantsInsertSingleRecordSetting_ShouldSucceedForSeparateTenants()
    {
        // Arrange
        using var context = CreateDbContext();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var settingA = PomodoroSetting.CreateDefault(Guid.NewGuid(), tenantA);
        context.PomodoroSettings.Add(settingA);
        await context.SaveChangesAsync();

        // Act: tenant B inserts its own setting
        var settingB = PomodoroSetting.CreateDefault(Guid.NewGuid(), tenantB);
        context.PomodoroSettings.Add(settingB);
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);

        // But attempting a second record for tenant B must fail
        var duplicateSettingB = PomodoroSetting.CreateDefault(Guid.NewGuid(), tenantB);
        context.PomodoroSettings.Add(duplicateSettingB);

        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(string.Format(SingleRecordSettingInterceptor.DuplicateErrorMessage, nameof(PomodoroSetting)));
    }

    [Fact]
    public void Synchronous_SaveChanges_AlsoEnforcesSingleRecordConstraint()
    {
        // Arrange
        using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var first = PomodoroSetting.CreateDefault(userId, tenantId);
        context.PomodoroSettings.Add(first);
        context.SaveChanges();

        // Act: try adding a second record synchronously
        var second = PomodoroSetting.CreateDefault(userId, tenantId);
        context.PomodoroSettings.Add(second);

        var act = () => context.SaveChanges();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage(string.Format(SingleRecordSettingInterceptor.DuplicateErrorMessage, nameof(PomodoroSetting)));
    }

    [Fact]
    public async Task When_MongoDocumentImplementsISingleRecordSetting_AndRecordExists_AddAsync_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var collectionMock = new Mock<IMongoCollection<IdentitySetting>>();
        var databaseMock = new Mock<IMongoDatabase>();
        databaseMock.Setup(db => db.GetCollection<IdentitySetting>("IdentitySettings", null))
            .Returns(collectionMock.Object);

        // Simulate 1 existing document
        collectionMock.Setup(c => c.CountDocumentsAsync(
                It.IsAny<FilterDefinition<IdentitySetting>>(),
                It.IsAny<CountOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var repo = new IdentitySettingRepository(databaseMock.Object);
        var setting = new IdentitySetting();

        // Act
        var act = async () => await repo.AddAsync(setting);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"An instance of single-record setting '{nameof(IdentitySetting)}' already exists. Only one record is permitted.");
    }

    [Fact]
    public async Task When_MongoDocumentImplementsISingleRecordSetting_AndNoRecordExists_AddAsync_ShouldSucceed()
    {
        // Arrange
        var collectionMock = new Mock<IMongoCollection<IdentitySetting>>();
        var databaseMock = new Mock<IMongoDatabase>();
        databaseMock.Setup(db => db.GetCollection<IdentitySetting>("IdentitySettings", null))
            .Returns(collectionMock.Object);

        // Simulate 0 existing documents
        collectionMock.Setup(c => c.CountDocumentsAsync(
                It.IsAny<FilterDefinition<IdentitySetting>>(),
                It.IsAny<CountOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        collectionMock.Setup(c => c.InsertOneAsync(
                It.IsAny<IdentitySetting>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var repo = new IdentitySettingRepository(databaseMock.Object);
        var setting = new IdentitySetting();

        // Act
        await repo.AddAsync(setting);

        // Assert
        collectionMock.Verify(c => c.InsertOneAsync(setting, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
