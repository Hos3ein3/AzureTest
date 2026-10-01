

using System;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

/// <summary>
/// Read-only projection representing a status change log entry for a Resume.
/// </summary>
public sealed record ResumeStatusLogDto(
    Guid Id,
    Guid ResumeId,
    DateTime Date,
    ResumeStatus OldStatus,
    ResumeStatus NewStatus,
    string? Note);
