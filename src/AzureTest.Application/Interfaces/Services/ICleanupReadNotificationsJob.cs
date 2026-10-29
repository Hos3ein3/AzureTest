using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface ICleanupReadNotificationsJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
