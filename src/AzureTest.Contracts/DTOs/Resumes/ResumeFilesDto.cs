

using System;

namespace AzureTest.Contracts.DTOs.Resumes;

/// <summary>
/// Read-only DTO that provides metadata about the uploaded files for a Resume.
/// </summary>
public sealed record ResumeFilesDto(
    Guid FileRecordId,
    Guid ResumeId,
    bool HasCv,
    string? CvName,
    bool HasCoverLetter,
    string? CoverLetterName,
    DateTime UploadedOn);
