using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Entities;
using System;

namespace AzureTest.Application.Resumes.Queries.GetResumeAnalytics;

public sealed record GetResumeAnalyticsQuery(string? UserId = null) : IQuery<ResumeAnalyticsDto>;

public sealed class GetResumeAnalyticsQueryHandler : BaseQueryHandler<GetResumeAnalyticsQuery, ResumeAnalyticsDto>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUser _currentUser;

    public GetResumeAnalyticsQueryHandler(
        IResumeRepository resumeRepository,
        ICompanyRepository companyRepository,
        ICurrentUser currentUser,
        ILogger<GetResumeAnalyticsQueryHandler> logger)
        : base(logger)
    {
        _resumeRepository = resumeRepository;
        _companyRepository = companyRepository;
        _currentUser = currentUser;
    }

    protected override async Task<ResumeAnalyticsDto> ExecuteAsync(GetResumeAnalyticsQuery query, CancellationToken cancellationToken)
    {
        var allResumes = await _resumeRepository.GetAllAsync();
        
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        var totalUsersCount = allResumes.Select(r => r.UserId).Distinct().Count();

        List<Resume> resumes;
        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(query.UserId) && Guid.TryParse(query.UserId, out var filterGuid))
            {
                resumes = allResumes.Where(r => r.UserId == filterGuid).ToList();
            }
            else
            {
                resumes = allResumes.ToList();
            }
        }
        else
        {
            resumes = allResumes.Where(r => r.UserId == _currentUser.Id).ToList();
        }

        var statusCounts = resumes
            .GroupBy(r => r.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        
        var dateCounts = resumes
            .GroupBy(r => r.CreatedOn.ToLocalTime().ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
            
        var positionCounts = resumes
            .GroupBy(r => string.IsNullOrWhiteSpace(r.PositionTitle) ? "Unknown" : r.PositionTitle)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
            
        var locationCounts = resumes
            .GroupBy(r => string.IsNullOrWhiteSpace(r.Location) ? "Unknown" : r.Location)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        var sourceCounts = resumes
            .GroupBy(r => r.Source.ToString())
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        int totalApps = resumes.Count;
        int activeApps = resumes.Count(r => 
            r.Status != AzureTest.Domain.Enums.ResumeStatus.Rejected && 
            r.Status != AzureTest.Domain.Enums.ResumeStatus.Ghosted && 
            r.Status != AzureTest.Domain.Enums.ResumeStatus.Withdraw && 
            r.Status != AzureTest.Domain.Enums.ResumeStatus.OfferAccepted);
        
        int interviewCount = resumes.Count(r => 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.Interview || 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.Offer || 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.OfferExtended || 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.OfferAccepted);
        double interviewRate = totalApps > 0 ? Math.Round((double)interviewCount / totalApps * 100, 1) : 0;

        int offerCount = resumes.Count(r => 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.Offer || 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.OfferExtended || 
            r.Status == AzureTest.Domain.Enums.ResumeStatus.OfferAccepted);
        double offerRate = totalApps > 0 ? Math.Round((double)offerCount / totalApps * 100, 1) : 0;

        var distinctCompanyIds = resumes.Where(r => r.CompanyId.HasValue).Select(r => r.CompanyId.Value).Distinct().ToList();
        var companiesCount = await _companyRepository.GetAllAvailableAsync();
        

        // Fetch recent activities
        var recentResumes = resumes.OrderByDescending(r => r.CreatedOn).Take(5).ToList();
        var allCompanies = await _companyRepository.GetAllAsync();
        var companyDict = allCompanies.ToDictionary(c => c.Id, c => c.CompanyName);

        var recentActivities = recentResumes.Select(r => new RecentActivityDto(
            r.Id,
            string.IsNullOrWhiteSpace(r.PositionTitle) ? "Unknown Position" : r.PositionTitle,
            r.CompanyId.HasValue && companyDict.ContainsKey(r.CompanyId.Value) ? companyDict[r.CompanyId.Value] : "Unknown Company",
            r.Status.ToString(),
            r.CreatedOn
        )).ToList();

        return new ResumeAnalyticsDto(
            statusCounts, 
            dateCounts, 
            positionCounts, 
            locationCounts, 
            sourceCounts, 
            totalApps, 
            activeApps, 
            interviewRate,
            companiesCount.Count(),
            offerRate,
            recentActivities,
            totalUsersCount,
            isAdmin);
    }
}
