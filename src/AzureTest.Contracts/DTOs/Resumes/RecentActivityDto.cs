

using System;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record RecentActivityDto(
    Guid ResumeId,
    string PositionTitle,
    string CompanyName,
    string Status,
    DateTime Date
);
