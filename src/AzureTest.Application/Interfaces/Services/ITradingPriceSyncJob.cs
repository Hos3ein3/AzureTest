using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface ITradingPriceSyncJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
