

using System;
using System.IO;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record UploadResumeFilesDto(
    Guid ResumeId,
    Stream? CvContent,
    string? CvFileName,
    string? CvContentType,
    Stream? ClContent,
    string? ClFileName,
    string? ClContentType);
