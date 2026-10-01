

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Services;

public class ResumeDocumentService : IResumeDocumentService
{
    private readonly IResumeFilesRepository _resumeFilesRepository;
    private readonly IBlobService _blobService;
    private readonly ICurrentUser _currentUser;

    public ResumeDocumentService(IResumeFilesRepository resumeFilesRepository, IBlobService blobService, ICurrentUser currentUser)
    {
        _resumeFilesRepository = resumeFilesRepository;
        _blobService = blobService;
        _currentUser = currentUser;
    }

    public async Task<Guid> UploadFilesAsync(UploadResumeFilesDto dto, CancellationToken cancellationToken = default)
    {
        bool isNew = false;
        var document = await _resumeFilesRepository.GetByResumeIdAsync(dto.ResumeId);
        if (document is null)
        {
            document = new ResumeFiles { ResumeId = dto.ResumeId, UserId = _currentUser.Id };
            isNew = true;
        }
        else if (document.UserId == Guid.Empty)
        {
            document.UserId = _currentUser.Id; // Backfill if empty
        }

        string containerName = $"resumes/{document.UserId}";

        if (dto.CvContent is not null && !string.IsNullOrWhiteSpace(dto.CvFileName))
        {
            if (!string.IsNullOrWhiteSpace(document.TempNameOfCV))
                await _blobService.DeleteFileAsync(containerName, document.TempNameOfCV, cancellationToken);

            var ext = Path.GetExtension(dto.CvFileName);
            var tempName = $"{Guid.NewGuid()}{ext}";
            
            await _blobService.UploadFileAsync(containerName, tempName, dto.CvContent, dto.CvContentType ?? "application/octet-stream", cancellationToken);
            
            document.RealNameOfCV = dto.CvFileName;
            document.TempNameOfCV = tempName;
            document.CvSize = dto.CvContent.Length;
        }

        if (dto.ClContent is not null && !string.IsNullOrWhiteSpace(dto.ClFileName))
        {
            if (!string.IsNullOrWhiteSpace(document.TempNameOfCL))
                await _blobService.DeleteFileAsync(containerName, document.TempNameOfCL, cancellationToken);

            var ext = Path.GetExtension(dto.ClFileName);
            var tempName = $"{Guid.NewGuid()}{ext}";
            
            await _blobService.UploadFileAsync(containerName, tempName, dto.ClContent, dto.ClContentType ?? "application/octet-stream", cancellationToken);
            
            document.RealNameOfCL = dto.ClFileName;
            document.TempNameOfCL = tempName;
            document.ClSize = dto.ClContent.Length;
        }

        document.UploadedOn = DateTime.UtcNow;

        if (isNew)
        {
            await _resumeFilesRepository.AddAsync(document);
        }
        else
        {
            await _resumeFilesRepository.UpdateAsync(document.Id, document);
        }

        return document.Id;
    }

    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadFileAsync(Guid fileRecordId, string fileType, CancellationToken cancellationToken = default)
    {
        var fileRecord = await _resumeFilesRepository.GetByIdAsync(fileRecordId);
        if (fileRecord is null) return null;

        string containerName = fileRecord.UserId == Guid.Empty ? "resumes" : $"resumes/{fileRecord.UserId}";

        string? tempName = fileType.ToLower() == "cv" ? fileRecord.TempNameOfCV : fileRecord.TempNameOfCL;
        string? realName = fileType.ToLower() == "cv" ? fileRecord.RealNameOfCV : fileRecord.RealNameOfCL;

        if (string.IsNullOrWhiteSpace(tempName) || string.IsNullOrWhiteSpace(realName))
        {
            return null;
        }

        try
        {
            var stream = await _blobService.DownloadFileAsync(containerName, tempName, cancellationToken);
            
            // Determine Content-Type based on extension for preview to work in browser
            string extension = Path.GetExtension(realName).ToLowerInvariant();
            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            return (stream, contentType, realName);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResumeFilesDto?> GetFileMetadataByResumeIdAsync(Guid resumeId, CancellationToken cancellationToken = default)
    {
        var document = await _resumeFilesRepository.GetByResumeIdAsync(resumeId);
        if (document is null) return null;

        return new ResumeFilesDto(
            document.Id,
            document.ResumeId,
            !string.IsNullOrWhiteSpace(document.RealNameOfCV),
            document.RealNameOfCV,
            !string.IsNullOrWhiteSpace(document.RealNameOfCL),
            document.RealNameOfCL,
            document.UploadedOn
        );
    }

    public async Task DeleteFileAsync(Guid fileRecordId, string fileType, CancellationToken cancellationToken = default)
    {
        var document = await _resumeFilesRepository.GetByIdAsync(fileRecordId);
        if (document is null) return;

        string containerName = document.UserId == Guid.Empty ? "resumes" : $"resumes/{document.UserId}";

        if (fileType.ToLower() == "cv" && !string.IsNullOrWhiteSpace(document.TempNameOfCV))
        {
            await _blobService.DeleteFileAsync(containerName, document.TempNameOfCV, cancellationToken);
            document.TempNameOfCV = null;
            document.RealNameOfCV = null;
        }
        else if (fileType.ToLower() == "cl" && !string.IsNullOrWhiteSpace(document.TempNameOfCL))
        {
            await _blobService.DeleteFileAsync(containerName, document.TempNameOfCL, cancellationToken);
            document.TempNameOfCL = null;
            document.RealNameOfCL = null;
        }

        // If both files are now deleted, we can optionally remove the whole document,
        // or just keep it around. Since it's a 1-to-1 relationship, keeping the empty document is fine.
        // We'll update the record to clear out the deleted file names.
        await _resumeFilesRepository.UpdateAsync(document.Id, document);
    }

    public async Task DeleteAllFilesByResumeIdAsync(Guid resumeId, CancellationToken cancellationToken = default)
    {
        var document = await _resumeFilesRepository.GetByResumeIdAsync(resumeId);
        if (document is null) return;

        // Soft-delete: mark the document as deleted without removing it from MongoDB
        document.SoftDelete();
        await _resumeFilesRepository.UpdateAsync(document.Id, document);
    }

    public async Task<int> SyncAllFileSizesAsync(CancellationToken cancellationToken = default)
    {
        var allDocuments = await _resumeFilesRepository.GetAllAsync();
        int updatedCount = 0;

        foreach (var document in allDocuments)
        {
            bool hasUpdates = false;
            string containerName = document.UserId == Guid.Empty ? "resumes" : $"resumes/{document.UserId}";

            if (!string.IsNullOrWhiteSpace(document.TempNameOfCV) && document.CvSize == 0)
            {
                try
                {
                    using var stream = await _blobService.DownloadFileAsync(containerName, document.TempNameOfCV, cancellationToken);
                    document.CvSize = stream.Length;
                    hasUpdates = true;
                }
                catch
                {
                    // Ignore missing files or errors during sync
                }
            }

            if (!string.IsNullOrWhiteSpace(document.TempNameOfCL) && document.ClSize == 0)
            {
                try
                {
                    using var stream = await _blobService.DownloadFileAsync(containerName, document.TempNameOfCL, cancellationToken);
                    document.ClSize = stream.Length;
                    hasUpdates = true;
                }
                catch
                {
                    // Ignore missing files or errors during sync
                }
            }

            if (hasUpdates)
            {
                await _resumeFilesRepository.UpdateAsync(document.Id, document);
                updatedCount++;
            }
        }

        return updatedCount;
    }
}
