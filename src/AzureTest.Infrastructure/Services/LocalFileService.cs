

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace AzureTest.Infrastructure.Services;

/// <summary>
/// Local filesystem implementation of IBlobService.
/// Stores files under wwwroot/uploads/{containerName}/ when blob storage is disabled.
/// </summary>
public class LocalFileService : IBlobService
{
    private readonly string _uploadsRoot;

    public LocalFileService(IWebHostEnvironment env)
    {
        // Files go into: wwwroot/uploads/{containerName}/
        _uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
    }

    public async Task<string> UploadFileAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_uploadsRoot, containerName);
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, blobName);

        if (content.CanSeek)
            content.Position = 0;

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        // Return a relative URL that can be served by the static files middleware
        return $"/uploads/{containerName}/{blobName}";
    }

    public Task<Stream> DownloadFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_uploadsRoot, containerName, blobName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Local file not found: {blobName}", filePath);

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_uploadsRoot, containerName, blobName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
