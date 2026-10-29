using System;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureTest.UnitTest.Interceptors;

public class ConcurrencyStampInterceptorTests
{
    private sealed class ConcurrencyStampedEntity : IConcurrencyStamp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Guid ConcurrencyStamp { get; set; }
    }

    private sealed class StringStampedEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? ConcurrencyStamp { get; set; }
    }

    private sealed class PlainEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext : DbContext
    {
        public DbSet<ConcurrencyStampedEntity> StampedEntities => Set<ConcurrencyStampedEntity>();
        public DbSet<StringStampedEntity> StringStampedEntities => Set<StringStampedEntity>();
        public DbSet<PlainEntity> PlainEntities => Set<PlainEntity>();

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private static TestDbContext CreateDbContext(ConcurrencyStampInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task When_AddingEntityWithEmptyGuidStamp_ShouldAssignNewGuid()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new ConcurrencyStampedEntity
        {
            Name = "Initial",
            ConcurrencyStamp = Guid.Empty
        };

        context.StampedEntities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().NotBeEmpty();
    }

    [Fact]
    public async Task When_AddingEntityWithPredefinedStamp_ShouldPreserveExistingStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var existingStamp = Guid.NewGuid();
        var entity = new ConcurrencyStampedEntity
        {
            Name = "Initial",
            ConcurrencyStamp = existingStamp
        };

        context.StampedEntities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().Be(existingStamp);
    }

    [Fact]
    public async Task When_ModifyingEntityWithInterface_ShouldRotateConcurrencyStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new ConcurrencyStampedEntity
        {
            Name = "Original"
        };
        context.StampedEntities.Add(entity);
        await context.SaveChangesAsync();

        var initialStamp = entity.ConcurrencyStamp;
        initialStamp.Should().NotBeEmpty();

        // Act
        entity.Name = "Updated";
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().NotBe(initialStamp);
        entity.ConcurrencyStamp.Should().NotBeEmpty();
    }

    [Fact]
    public async Task When_DeletingEntityWithInterface_ShouldRotateConcurrencyStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new ConcurrencyStampedEntity
        {
            Name = "To Delete"
        };
        context.StampedEntities.Add(entity);
        await context.SaveChangesAsync();

        var initialStamp = entity.ConcurrencyStamp;

        // Act
        context.StampedEntities.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().NotBe(initialStamp);
    }

    [Fact]
    public async Task When_ModifyingStringStampedEntity_ShouldRotateConcurrencyStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new StringStampedEntity
        {
            Name = "Original",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        context.StringStampedEntities.Add(entity);
        await context.SaveChangesAsync();

        var originalStamp = entity.ConcurrencyStamp;

        // Act
        entity.Name = "Updated";
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().NotBe(originalStamp);
        Guid.TryParse(entity.ConcurrencyStamp, out _).Should().BeTrue();
    }

    [Fact]
    public void When_SavingSynchronously_ShouldRotateConcurrencyStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new ConcurrencyStampedEntity
        {
            Name = "Sync Original"
        };
        context.StampedEntities.Add(entity);
        context.SaveChanges();

        var initialStamp = entity.ConcurrencyStamp;

        // Act
        entity.Name = "Sync Updated";
        context.SaveChanges();

        // Assert
        entity.ConcurrencyStamp.Should().NotBe(initialStamp);
        entity.ConcurrencyStamp.Should().NotBeEmpty();
    }

    [Fact]
    public async Task When_EntityIsUnchanged_ShouldNotRotateConcurrencyStamp()
    {
        // Arrange
        var interceptor = new ConcurrencyStampInterceptor();
        using var context = CreateDbContext(interceptor);
        var entity = new ConcurrencyStampedEntity
        {
            Name = "Unchanged"
        };
        context.StampedEntities.Add(entity);
        await context.SaveChangesAsync();

        var initialStamp = entity.ConcurrencyStamp;

        // Act: Save without modifying
        await context.SaveChangesAsync();

        // Assert
        entity.ConcurrencyStamp.Should().Be(initialStamp);
    }

    [Fact]
    public async Task When_AuditInterceptorAloneIsUsed_ShouldNotRotateConcurrencyStamp()
    {
        // Arrange: DbContext with ONLY AuditInterceptor (demonstrating separation of concerns)
        var auditInterceptor = new AuditInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(auditInterceptor)
            .Options;

        using var context = new TestDbContext(options);
        var initialStamp = Guid.NewGuid().ToString();
        var entity = new StringStampedEntity
        {
            Name = "AuditOnly",
            ConcurrencyStamp = initialStamp
        };
        context.StringStampedEntities.Add(entity);
        await context.SaveChangesAsync();

        // Act: Modify entity and save with AuditInterceptor only
        entity.Name = "AuditOnly Updated";
        await context.SaveChangesAsync();

        // Assert: AuditInterceptor no longer touches ConcurrencyStamp
        entity.ConcurrencyStamp.Should().Be(initialStamp);
    }
}
