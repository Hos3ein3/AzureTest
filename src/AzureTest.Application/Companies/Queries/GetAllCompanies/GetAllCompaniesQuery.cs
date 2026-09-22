using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Companies.Queries.GetAllCompanies;

public sealed record GetAllCompaniesQuery(GetAllCompaniesFilters Filters) : IQuery<IEnumerable<CompanyDto>>;

public sealed class GetAllCompaniesQueryHandler : IQueryHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>>
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

    public async Task<IEnumerable<CompanyDto>> HandleAsync(GetAllCompaniesQuery request, CancellationToken cancellationToken)
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
        if (!System.String.IsNullOrEmpty(request.Filters?.Name))
        {
            return allDtos.Where(x => x.CompanyName.Contains(request.Filters.Name, System.StringComparison.OrdinalIgnoreCase));
        }

        return allDtos;
    }
}
