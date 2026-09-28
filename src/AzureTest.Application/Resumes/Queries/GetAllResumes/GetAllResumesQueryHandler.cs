using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Resumes.Queries.GetAllResumes;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetAllResumesQuery(GetAllResumesFilters Filters) : IQuery<PagedResult<ResumeDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAllResumesQueryHandler : BaseQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetAllResumesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<GetAllResumesQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<PagedResult<ResumeDto>> ExecuteAsync(GetAllResumesQuery query, CancellationToken cancellationToken)
    {
        var resumes = await _unitOfWork.Resumes.FindAsync(r => !r.IsDeleted && r.UserId == _currentUser.Id);

        // Apply filters in memory (Note: for large datasets, this should be moved to IQueryable in repo)
        var queryable = resumes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filters?.SearchTerm))
        {
            var term = query.Filters.SearchTerm.ToLower();
            queryable = queryable.Where(r => 
                (!string.IsNullOrEmpty(r.PositionTitle) && r.PositionTitle.ToLower().Contains(term)) ||
                (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Filters?.Location))
        {
            var loc = query.Filters.Location.ToLower();
            queryable = queryable.Where(r => 
                !string.IsNullOrEmpty(r.Location) && r.Location.ToLower().Contains(loc));
        }

        if (query.Filters?.Source.HasValue == true)
        {
            queryable = queryable.Where(r => r.Source == query.Filters.Source.Value);
        }

        if (query.Filters?.Status.HasValue == true)
        {
            queryable = queryable.Where(r => r.Status == query.Filters.Status.Value);
        }
        
        if (query.Filters?.AppliedDate.HasValue == true)
        {
            var date = query.Filters.AppliedDate.Value.Date;
            queryable = queryable.Where(r => r.CreatedOn.Date == date);
        }

        int totalCount = queryable.Count();
        int pageNumber = query.Filters?.PageNumber ?? 1;
        int pageSize = query.Filters?.PageSize ?? 10;
        
        // Sorting logic
        string sortBy = (query.Filters?.SortBy ?? "CreatedOn").ToLower();
        bool desc = query.Filters?.SortDescending ?? true;

        queryable = sortBy switch
        {
            "position" => desc ? queryable.OrderByDescending(r => r.PositionTitle) : queryable.OrderBy(r => r.PositionTitle),
            "location" => desc ? queryable.OrderByDescending(r => r.Location) : queryable.OrderBy(r => r.Location),
            "source" => desc ? queryable.OrderByDescending(r => r.Source) : queryable.OrderBy(r => r.Source),
            "status" => desc ? queryable.OrderByDescending(r => r.Status) : queryable.OrderBy(r => r.Status),
            _ => desc ? queryable.OrderByDescending(r => r.CreatedOn) : queryable.OrderBy(r => r.CreatedOn)
        };

        var pagedResumes = queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<ResumeDto>(pagedResumes, totalCount, pageNumber, pageSize);
    }

    private static ResumeDto MapToDto(Resume resume) => new(
        resume.Id,
        resume.PositionTitle,
        resume.Description,
        resume.Location,
        resume.JobLink,
        resume.Note,
        resume.ChatUrl,
        resume.Source,
        resume.Status,
        resume.CreatedOn,
        resume.ModifiedOn,
        resume.CompanyId,
        resume.AtsScore);
}
