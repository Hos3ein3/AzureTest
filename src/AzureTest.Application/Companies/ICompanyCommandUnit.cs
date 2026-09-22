using AzureTest.Application.Companies.Commands.CrawlIndCompanies;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Companies;

public interface ICompanyCommandUnit
{
    ICommandHandler<CrawlIndCompaniesCommand, CrawlIndCompaniesResult> CrawlCompanies { get; }
    ICommandHandler<CreateCompanyCommand,Guid> CreateCompany { get; }
    ICommandHandler<DeleteCompanyCommand,bool> DeleteCompany { get; }
    ICommandHandler<UpdateCompanyCommand,bool> UpdateCompany { get; }
}