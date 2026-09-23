using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Interfaces.Services;

public interface IResumeDocumentService
{
    Task<Guid> UploadFilesAsync(UploadResumeFilesDto dto, CancellationToken cancellationToken = default);
    Task<ResumeFilesDto?> GetFileMetadataByResumeIdAsync(Guid resumeId, CancellationToken cancellationToken = default);
    Task<(Stream Content, string ContentType, string FileName)?> DownloadFileAsync(Guid fileRecordId, string fileType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(Guid fileRecordId, string fileType, CancellationToken cancellationToken = default);
    Task DeleteAllFilesByResumeIdAsync(Guid resumeId, CancellationToken cancellationToken = default);
    Task<int> SyncAllFileSizesAsync(CancellationToken cancellationToken = default);
}
