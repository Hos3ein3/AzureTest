using System;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record ResumeFileDto(
    Guid Id,
    Guid ResumeId,
    string? RealNameOfCV,
    string? TempNameOfCV,
    string? RealNameOfCL,
    string? TempNameOfCL,
    DateTime UploadedOn);
