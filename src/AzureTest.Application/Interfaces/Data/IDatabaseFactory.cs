

using System;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data;

public interface IDatabaseFactory : IDisposable
{
    // A factory for providing active contexts or connections
    TContext GetContext<TContext>() where TContext : class;
}
