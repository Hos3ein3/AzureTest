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

    public GetAllCompaniesQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyDto>> HandleAsync(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Company> list;
        var companies = await _companyRepository.GetAllAsync();
        if (!String.IsNullOrEmpty(request.Filters.Name))
        {
            list = companies.Where(x => x.CompanyName.Contains(request.Filters.Name));
        }
        else
        {
            list = companies;
        }
        return list.Select(c => new CompanyDto(c.Id, c.CompanyName ?? "", c.KvkNumber ?? "", c.IsINDMember,c.Location));
    }
}
