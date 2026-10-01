

using AzureTest.Domain.Enums;

namespace AzureTest.Application.Resumes.Queries.GetAllResumes;

public sealed record GetAllResumesFilters(
    string? SearchTerm = null,
    string? Location = null,
    ResumeSource? Source = null,
    ResumeStatus? Status = null,
    DateTime? AppliedDate = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
);
