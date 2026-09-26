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
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly AzureTest.Application.Interfaces.Services.ICompanyCacheService _cacheService;

    public DeleteCompanyCommandHandler(
        IUnitOfWork unitOfWork, 
        INoSqlUnitOfWork noSqlUnitOfWork, 
        AzureTest.Application.Interfaces.Services.ICompanyCacheService cacheService,
        ILogger<DeleteCompanyCommandHandler> logger) : base(unitOfWork,logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cacheService = cacheService;
    }

    protected override async Task<bool> ExecuteAsync(DeleteCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _noSqlUnitOfWork.Companies.GetByIdAsync(command.Id);
        if (company == null) return false;

        await _noSqlUnitOfWork.Companies.RemoveAsync(company.Id);
        await _cacheService.InvalidateCompaniesAsync(cancellationToken);

        return true;
    }
}
