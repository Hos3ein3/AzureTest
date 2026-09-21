using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AzureTestDbContext _dbContext;

    public UnitOfWork(AzureTestDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns domain events collected by the DomainEventsInterceptor after the last SaveChanges.
    /// Call this after SaveChangesAsync to dispatch events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        return _dbContext.DomainEventsInterceptor.PopDomainEvents();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}

