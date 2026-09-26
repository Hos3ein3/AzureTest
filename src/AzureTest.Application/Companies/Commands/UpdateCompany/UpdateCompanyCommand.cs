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
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly AzureTest.Application.Interfaces.Services.ICompanyCacheService _cacheService;

    public UpdateCompanyCommandHandler(
        IUnitOfWork unitOfWork, 
        INoSqlUnitOfWork noSqlUnitOfWork, 
        AzureTest.Application.Interfaces.Services.ICompanyCacheService cacheService,
        ILogger<UpdateCompanyCommandHandler> logger) : base(unitOfWork,logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cacheService = cacheService;
    }

    protected override async Task<bool> ExecuteAsync(UpdateCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _noSqlUnitOfWork.Companies.GetByIdAsync(command.Dto.Id);
        if (company == null) return false;

        company.CompanyName = command.Dto.CompanyName;
        company.KvkNumber = command.Dto.KvkNumber;
        company.IsINDMember = command.Dto.IsINDMember;
        company.Location = command.Dto.Location;

        await _noSqlUnitOfWork.Companies.UpdateAsync(company.Id,company);
        await _cacheService.InvalidateCompaniesAsync(cancellationToken);

        return true;
    }
}
