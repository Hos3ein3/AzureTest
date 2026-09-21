using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface IBlobService
{
    Task<string> UploadFileAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
}
