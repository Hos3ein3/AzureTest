using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Companies.Commands.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid Id) : ICommand<bool>;

public sealed class DeleteCompanyCommandHandler : BaseCommandHandler<DeleteCompanyCommand, bool>
{
    private readonly ICompanyRepository _companyRepository;

    public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork, ICompanyRepository companyRepository,ILogger<DeleteCompanyCommandHandler> logger) : base(unitOfWork,logger)
    {
        _companyRepository = companyRepository;
    }

    

    protected override async Task<bool> ExecuteAsync(DeleteCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(command.Id);
        if (company == null) return false;

        await _companyRepository.RemoveAsync(company.Id);

        return true;
    }
}
