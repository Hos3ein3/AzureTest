using System.Collections.Generic;
using AzureTest.Application.Companies.Queries.GetAllCompanies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Companies;

public interface ICompanyQueryUnit
{
    public IQueryHandler<GetAllCompaniesQuery,PagedResult<CompanyDto>> GetAll { get;  }
    public IQueryHandler<GetCompanyByIdQuery,CompanyDto> GetById { get; }
}