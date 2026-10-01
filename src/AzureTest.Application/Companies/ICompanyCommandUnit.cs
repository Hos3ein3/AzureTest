using AzureTest.Application.Abstractions;


using AzureTest.Application.Companies.Commands.CrawlIndCompanies;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Companies;

public interface ICompanyCommandUnit
{
    ICommandHandler<CrawlIndCompaniesCommand, Result<CrawlIndCompaniesResult>> CrawlCompanies { get; }
    ICommandHandler<CreateCompanyCommand, Result<Guid>> CreateCompany { get; }
    ICommandHandler<DeleteCompanyCommand, Result<bool>> DeleteCompany { get; }
    ICommandHandler<UpdateCompanyCommand, Result<bool>> UpdateCompany { get; }
}