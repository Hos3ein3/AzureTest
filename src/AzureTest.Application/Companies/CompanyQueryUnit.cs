using AzureTest.Application.Companies.Queries.GetAllCompanies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;

namespace AzureTest.Application.Companies;

public sealed class CompanyQueryUnit:ICompanyQueryUnit
{
    public IQueryHandler<GetCompanyByIdQuery, CompanyDto> GetById { get; }
    public IQueryHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>> GetAll { get; }

    public CompanyQueryUnit(IQueryHandler<GetCompanyByIdQuery, CompanyDto> getById,IQueryHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>> getAll)
    {
        GetById = getById;
        GetAll = getAll;
    }
}