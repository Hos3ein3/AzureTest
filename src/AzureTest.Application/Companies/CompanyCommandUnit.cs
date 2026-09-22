using AzureTest.Application.Companies.Commands.CrawlIndCompanies;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Companies;

public class CompanyCommandUnit : ICompanyCommandUnit
{
    public ICommandHandler<CrawlIndCompaniesCommand, CrawlIndCompaniesResult> CrawlCompanies { get; }
    public ICommandHandler<CreateCompanyCommand, Guid> CreateCompany { get; }
    public ICommandHandler<DeleteCompanyCommand, bool> DeleteCompany { get; }
    public ICommandHandler<UpdateCompanyCommand, bool> UpdateCompany { get; }

    public CompanyCommandUnit(ICommandHandler<CrawlIndCompaniesCommand, CrawlIndCompaniesResult> crawlCompanies, ICommandHandler<CreateCompanyCommand, Guid> createCompany, ICommandHandler<DeleteCompanyCommand, bool> deleteCompany, ICommandHandler<UpdateCompanyCommand, bool> updateCompany)
    {
        CrawlCompanies = crawlCompanies;
        CreateCompany = createCompany;
        DeleteCompany = deleteCompany;
        UpdateCompany = updateCompany;
    }
}