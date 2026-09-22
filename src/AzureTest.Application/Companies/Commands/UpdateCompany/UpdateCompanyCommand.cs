using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Companies.Commands.UpdateCompany;

public sealed record UpdateCompanyCommand(UpdateCompanyDto Dto) : ICommand<bool>;

public sealed class UpdateCompanyCommandHandler : BaseCommandHandler<UpdateCompanyCommand, bool>
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, ICompanyRepository companyRepository, ILogger<UpdateCompanyCommandHandler> logger) : base(unitOfWork,logger)
    {
        _companyRepository = companyRepository;
    }


    protected override async Task<bool> ExecuteAsync(UpdateCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(command.Dto.Id);
        if (company == null) return false;

        company.CompanyName = command.Dto.CompanyName;
        company.KvkNumber = command.Dto.KvkNumber;
        company.IsINDMember = command.Dto.IsINDMember;
        company.Location = command.Dto.Location;

        await _companyRepository.UpdateAsync(company.Id,company);

        return true;
    }
}
