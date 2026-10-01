

using System;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Persistence.Factories;

public class DatabaseFactory : IDatabaseFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TContext GetContext<TContext>() where TContext : class
    {
        return _serviceProvider.GetRequiredService<TContext>();
    }

    public void Dispose()
    {
        // Clean up any context if managed directly by the factory
        GC.SuppressFinalize(this);
    }
}
