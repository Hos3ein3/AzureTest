

using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record ResumeAnalyticsDto(
    Dictionary<string, int> StatusCounts,
    Dictionary<string, int> ApplicationsByDate,
    Dictionary<string, int> PositionCounts,
    Dictionary<string, int> LocationCounts,
    Dictionary<string, int> SourceCounts,
    int TotalApplications,
    int ActiveApplications,
    double InterviewRate,
    int TotalCompanies,
    double OfferRate,
    List<RecentActivityDto> RecentActivities,
    int TotalUsers,
    bool IsAdmin,
    long OccupiedStorage = 0
);
