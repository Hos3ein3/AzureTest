using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// Intercepts EF Core SaveChanges operations to ensure that entities implementing ISingleRecordSetting
/// can only have a single record per tenant workspace (or globally if not a tenant entity).
/// Prevents inserting a new record if one already exists in the database or change tracker.
/// </summary>
public sealed class SingleRecordSettingInterceptor : SaveChangesInterceptor
{
    public const string DuplicateErrorMessage =
        "An instance of single-record setting '{0}' already exists. Only one record is permitted.";

    private static readonly MethodInfo CheckDbExistsGenericMethod = typeof(SingleRecordSettingInterceptor)
        .GetMethod(nameof(CheckDbExistsGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CheckDbExistsGenericAsyncMethod = typeof(SingleRecordSettingInterceptor)
        .GetMethod(nameof(CheckDbExistsGenericAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await ValidateSingleRecordSettingsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ValidateSingleRecordSettings(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ValidateSingleRecordSettings(DbContext context)
    {
        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is ISingleRecordSetting)
            .ToList();

        if (addedEntries.Count == 0) return;

        var grouped = addedEntries.GroupBy(e => e.Entity.GetType());

        foreach (var group in grouped)
        {
            var entityType = group.Key;

            // Multiple entries of this single-record type in the same SaveChanges batch
            if (group.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(DuplicateErrorMessage, entityType.Name));
            }

            var entry = group.First();

            if (CheckRecordExists(context, entry))
            {
                throw new InvalidOperationException(string.Format(DuplicateErrorMessage, entityType.Name));
            }
        }
    }

    private async Task ValidateSingleRecordSettingsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is ISingleRecordSetting)
            .ToList();

        if (addedEntries.Count == 0) return;

        var grouped = addedEntries.GroupBy(e => e.Entity.GetType());

        foreach (var group in grouped)
        {
            var entityType = group.Key;

            if (group.Count() > 1)
            {
                throw new InvalidOperationException(string.Format(DuplicateErrorMessage, entityType.Name));
            }

            var entry = group.First();

            if (await CheckRecordExistsAsync(context, entry, cancellationToken))
            {
                throw new InvalidOperationException(string.Format(DuplicateErrorMessage, entityType.Name));
            }
        }
    }

    private static (string KeyName, object? KeyValue)? GetPrimaryKey(EntityEntry entry)
    {
        var pk = entry.Metadata.FindPrimaryKey();
        if (pk != null && pk.Properties.Count == 1)
        {
            var propName = pk.Properties[0].Name;
            var val = entry.Property(propName).CurrentValue;
            return (propName, val);
        }
        return null;
    }

    private static bool CheckRecordExists(DbContext context, EntityEntry entry)
    {
        var entityType = entry.Entity.GetType();

        // 1. In-memory ChangeTracker check (excluding the current added entity)
        var inMemoryExists = context.ChangeTracker.Entries()
            .Any(e => !ReferenceEquals(e.Entity, entry.Entity) &&
                      e.Entity.GetType() == entityType &&
                      e.State is EntityState.Unchanged or EntityState.Modified or EntityState.Added &&
                      (!typeof(ITenantEntity).IsAssignableFrom(entityType) ||
                       ((ITenantEntity)e.Entity).TenantId == ((ITenantEntity)entry.Entity).TenantId));

        if (inMemoryExists)
        {
            return true;
        }

        // 2. Database query excluding the entity's own primary key
        var pkInfo = GetPrimaryKey(entry);
        var method = CheckDbExistsGenericMethod.MakeGenericMethod(entityType);
        return (bool)method.Invoke(null, new object?[] { context, entry.Entity, pkInfo?.KeyName, pkInfo?.KeyValue })!;
    }

    private static async Task<bool> CheckRecordExistsAsync(DbContext context, EntityEntry entry, CancellationToken cancellationToken)
    {
        var entityType = entry.Entity.GetType();

        var inMemoryExists = context.ChangeTracker.Entries()
            .Any(e => !ReferenceEquals(e.Entity, entry.Entity) &&
                      e.Entity.GetType() == entityType &&
                      e.State is EntityState.Unchanged or EntityState.Modified or EntityState.Added &&
                      (!typeof(ITenantEntity).IsAssignableFrom(entityType) ||
                       ((ITenantEntity)e.Entity).TenantId == ((ITenantEntity)entry.Entity).TenantId));

        if (inMemoryExists)
        {
            return true;
        }

        var pkInfo = GetPrimaryKey(entry);
        var method = CheckDbExistsGenericAsyncMethod.MakeGenericMethod(entityType);
        return await (Task<bool>)method.Invoke(null, new object?[] { context, entry.Entity, pkInfo?.KeyName, pkInfo?.KeyValue, cancellationToken })!;
    }

    private static bool CheckDbExistsGeneric<T>(DbContext context, object entity, string? keyPropName, object? keyValue) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? filter = null;

        if (!string.IsNullOrEmpty(keyPropName) && keyValue != null)
        {
            var keyPropertyAccess = Expression.Property(parameter, keyPropName);
            var notEqual = Expression.NotEqual(keyPropertyAccess, Expression.Constant(keyValue, keyPropertyAccess.Type));
            filter = notEqual;
        }

        if (entity is ITenantEntity tenantEntity && typeof(ITenantEntity).IsAssignableFrom(typeof(T)) && tenantEntity.TenantId != Guid.Empty)
        {
            var tenantPropertyAccess = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
            var tenantEqual = Expression.Equal(tenantPropertyAccess, Expression.Constant(tenantEntity.TenantId));
            filter = filter != null ? Expression.AndAlso(filter, tenantEqual) : tenantEqual;
        }

        if (filter != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(filter, parameter);
            return context.Set<T>().Any(lambda);
        }

        return context.Set<T>().Any();
    }

    private static async Task<bool> CheckDbExistsGenericAsync<T>(DbContext context, object entity, string? keyPropName, object? keyValue, CancellationToken cancellationToken) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? filter = null;

        if (!string.IsNullOrEmpty(keyPropName) && keyValue != null)
        {
            var keyPropertyAccess = Expression.Property(parameter, keyPropName);
            var notEqual = Expression.NotEqual(keyPropertyAccess, Expression.Constant(keyValue, keyPropertyAccess.Type));
            filter = notEqual;
        }

        if (entity is ITenantEntity tenantEntity && typeof(ITenantEntity).IsAssignableFrom(typeof(T)) && tenantEntity.TenantId != Guid.Empty)
        {
            var tenantPropertyAccess = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
            var tenantEqual = Expression.Equal(tenantPropertyAccess, Expression.Constant(tenantEntity.TenantId));
            filter = filter != null ? Expression.AndAlso(filter, tenantEqual) : tenantEqual;
        }

        if (filter != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(filter, parameter);
            return await context.Set<T>().AnyAsync(lambda, cancellationToken);
        }

        return await context.Set<T>().AnyAsync(cancellationToken);
    }
}
