using AzureTest.Application.Companies.Queries.GetAllCompanies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Companies;

public sealed class CompanyQueryUnit:ICompanyQueryUnit
{
    public IQueryHandler<GetCompanyByIdQuery, CompanyDto> GetById { get; }
    public IQueryHandler<GetAllCompaniesQuery, PagedResult<CompanyDto>> GetAll { get; }

    public CompanyQueryUnit(IQueryHandler<GetCompanyByIdQuery, CompanyDto> getById,IQueryHandler<GetAllCompaniesQuery, PagedResult<CompanyDto>> getAll)
    {
        GetById = getById;
        GetAll = getAll;
    }
}