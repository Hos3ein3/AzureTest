using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record ResumeAnalyticsDto(
    Dictionary<string, int> StatusCounts,
    Dictionary<string, int> ApplicationsByDate
);
