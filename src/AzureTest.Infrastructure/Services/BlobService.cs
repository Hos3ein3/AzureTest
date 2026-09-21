using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace AzureTest.Infrastructure.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BlobStorage");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("BlobStorage connection string is not configured.");
        }
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadFileAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };
        
        // Ensure stream is at the beginning
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await blobClient.UploadAsync(content, options, cancellationToken);
        
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteFileAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
