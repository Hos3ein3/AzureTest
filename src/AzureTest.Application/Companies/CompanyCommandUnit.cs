using AzureTest.Application.Abstractions;


using AzureTest.Application.Companies.Commands.CrawlIndCompanies;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Companies;

public class CompanyCommandUnit : ICompanyCommandUnit
{
    public ICommandHandler<CrawlIndCompaniesCommand, Result<CrawlIndCompaniesResult>> CrawlCompanies { get; }
    public ICommandHandler<CreateCompanyCommand, Result<Guid>> CreateCompany { get; }
    public ICommandHandler<DeleteCompanyCommand, Result<bool>> DeleteCompany { get; }
    public ICommandHandler<UpdateCompanyCommand, Result<bool>> UpdateCompany { get; }
    public CompanyCommandUnit(ICommandHandler<CrawlIndCompaniesCommand, Result<CrawlIndCompaniesResult>> crawlCompanies, ICommandHandler<CreateCompanyCommand, Result<Guid>> createCompany, ICommandHandler<DeleteCompanyCommand, Result<bool>> deleteCompany, ICommandHandler<UpdateCompanyCommand, Result<bool>> updateCompany)
    {
        CrawlCompanies = crawlCompanies;
        CreateCompany = createCompany;
        DeleteCompany = deleteCompany;
        UpdateCompany = updateCompany;
    }
}