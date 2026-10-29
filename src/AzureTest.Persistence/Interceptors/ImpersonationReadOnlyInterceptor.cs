using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// Intercepts EF Core SaveChanges operations to ensure that when an administrator is impersonating
/// a tenant workspace, mutations to tenant data (ITenantEntity) cannot be persisted if impersonation
/// write operations are prohibited by feature management.
/// </summary>
public sealed class ImpersonationReadOnlyInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration? _configuration;

    public const string ReadOnlyErrorMessage =
        "Workspace is in Read-Only Impersonation mode. Administrative write operations (create, update, delete) are prohibited while impersonating another tenant.";

    public ImpersonationReadOnlyInterceptor(
        ITenantProvider tenantProvider,
        IConfiguration? configuration = null)
    {
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        _configuration = configuration;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidateImpersonationReadOnly(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateImpersonationReadOnly(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ValidateImpersonationReadOnly(DbContext? context)
    {
        if (context == null) return;

        // If AllowImpersonationWrites from feature management is true, admin can perform write operations:
        if (IsAllowImpersonationWritesEnabled())
        {
            return;
        }

        // When admin is impersonating another tenant workspace:
        if (_tenantProvider.IsAdminImpersonating)
        {
            var hasTenantMutations = context.ChangeTracker.Entries<ITenantEntity>()
                .Any(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

            if (hasTenantMutations)
            {
                throw new InvalidOperationException(ReadOnlyErrorMessage);
            }
        }
    }

    private bool IsAllowImpersonationWritesEnabled()
    {
        if (_tenantProvider.AllowImpersonationWrites)
        {
            return true;
        }

        if (_configuration != null)
        {
            if (_configuration.GetValue<bool>("FeatureManagement:AllowImpersonationWrites") ||
                _configuration.GetValue<bool>("InfrastructureFeatures:AllowImpersonationWrites"))
            {
                return true;
            }
        }

        return false;
    }
}
