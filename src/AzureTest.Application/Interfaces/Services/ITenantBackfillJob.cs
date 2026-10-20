using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface ITenantBackfillJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
