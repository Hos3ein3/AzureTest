using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;

namespace AzureTest.Application.Companies.Queries.GetCompanyById;

public sealed record GetCompanyByIdQuery(Guid Id) : IQuery<CompanyDto?>;

public sealed class GetCompanyByIdQueryHandler : IQueryHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetCompanyByIdQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<CompanyDto?> HandleAsync(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _noSqlUnitOfWork.Companies.GetByIdAsync(request.Id);
        if (company == null) return null;

        return new CompanyDto(company.Id, company.CompanyName ?? "", company.KvkNumber ?? "", company.IsINDMember,company.Location);
    }
}
