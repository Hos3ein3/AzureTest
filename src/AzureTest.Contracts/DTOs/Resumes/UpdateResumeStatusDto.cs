using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record UpdateResumeStatusDto(
    Guid Id,
    ResumeStatus Status,
    string? Note = null);
