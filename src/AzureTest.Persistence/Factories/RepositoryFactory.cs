

using System;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Persistence.Factories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IGenericRepository<TEntity> Instantiate<TEntity>() where TEntity : BaseEntity
    {
        return _serviceProvider.GetRequiredService<IGenericRepository<TEntity>>();
    }
}
