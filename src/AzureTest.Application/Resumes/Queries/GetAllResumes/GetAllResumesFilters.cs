using System;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Resumes.Queries.GetAllResumes;

public class GetAllResumesFilters : PagedFilterRequest
{
    public string? Location { get; set; }
    public ResumeSource? Source { get; set; }
    public ResumeStatus? Status { get; set; }
    public DateTime? AppliedDate { get; set; }

    public GetAllResumesFilters()
    {
        SortBy = "CreatedOn";
        SortDescending = true;
    }

    public GetAllResumesFilters(
        string? SearchTerm = null,
        string? Location = null,
        ResumeSource? Source = null,
        ResumeStatus? Status = null,
        DateTime? AppliedDate = null,
        string? SortBy = "CreatedOn",
        bool SortDescending = true,
        int PageNumber = 1,
        int PageSize = 10
    )
    {
        this.SearchTerm = SearchTerm;
        this.Location = Location;
        this.Source = Source;
        this.Status = Status;
        this.AppliedDate = AppliedDate;
        this.SortBy = SortBy ?? "CreatedOn";
        this.SortDescending = SortDescending;
        this.PageNumber = PageNumber;
        this.PageSize = PageSize;
    }
}
