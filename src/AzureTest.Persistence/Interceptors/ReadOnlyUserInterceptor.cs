using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// Intercepts SaveChanges and prevents any data modifications if the current authenticated user
/// has been marked as IsReadOnly.
/// </summary>
public sealed class ReadOnlyUserInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public ReadOnlyUserInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidateNotReadOnly(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateNotReadOnly(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ValidateNotReadOnly(DbContext? context)
    {
        if (context == null) return;

        if (_currentUser != null && _currentUser.IsReadOnly)
        {
            var hasMutations = context.ChangeTracker.Entries()
                .Any(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

            if (hasMutations)
            {
                throw new InvalidOperationException("Your user account is in Read-Only mode. You cannot save or modify data.");
            }
        }
    }
}
