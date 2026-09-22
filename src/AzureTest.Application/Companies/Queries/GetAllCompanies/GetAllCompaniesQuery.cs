using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Companies.Queries.GetAllCompanies;

public sealed record GetAllCompaniesQuery(GetAllCompaniesFilters Filters) : IQuery<PagedResult<CompanyDto>>;

public sealed class GetAllCompaniesQueryHandler : IQueryHandler<GetAllCompaniesQuery, PagedResult<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly AzureTest.Application.Interfaces.Services.ICompanyCacheService _cacheService;

    public GetAllCompaniesQueryHandler(
        ICompanyRepository companyRepository,
        AzureTest.Application.Interfaces.Services.ICompanyCacheService cacheService)
    {
        _companyRepository = companyRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<CompanyDto>> HandleAsync(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        // Check cache first
        var cachedCompanies = await _cacheService.GetCompaniesAsync(cancellationToken);
        
        IEnumerable<CompanyDto> allDtos;
        if (cachedCompanies != null)
        {
            allDtos = cachedCompanies;
        }
        else
        {
            var companies = await _companyRepository.GetAllAsync();
            allDtos = companies.Select(c => new CompanyDto(c.Id, c.CompanyName ?? "", c.KvkNumber ?? "", c.IsINDMember, c.Location));
            
            // Set in cache
            await _cacheService.SetCompaniesAsync(allDtos, cancellationToken);
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Filters?.Name))
        {
            allDtos = allDtos.Where(x => x.CompanyName.Contains(request.Filters.Name, System.StringComparison.OrdinalIgnoreCase));
        }
        if (request.Filters?.IsINDMember.HasValue == true)
        {
            allDtos = allDtos.Where(x => x.IsINDMember == request.Filters.IsINDMember.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.Filters?.Location))
        {
            allDtos = allDtos.Where(x => !string.IsNullOrWhiteSpace(x.Location) && 
                                         x.Location.Contains(request.Filters.Location, System.StringComparison.OrdinalIgnoreCase));
        }

        int totalCount = allDtos.Count();
        int pageNumber = request.Filters?.PageNumber ?? 1;
        int pageSize = request.Filters?.PageSize ?? 10;
        
        var pagedItems = allDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CompanyDto>(pagedItems, totalCount, pageNumber, pageSize);
    }
}
