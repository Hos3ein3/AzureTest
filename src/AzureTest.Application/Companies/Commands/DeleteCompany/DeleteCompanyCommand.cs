using AzureTest.Application.Abstractions;

using FluentValidation;

using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Companies.Commands.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid Id) : ICommand<Result<bool>>;
public sealed class DeleteCompanyCommandHandler : BaseCommandHandler<DeleteCompanyCommand, Result<bool>>
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
    protected override async Task<Result<bool>> ExecuteAsync(DeleteCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _noSqlUnitOfWork.Companies.GetByIdAsync(command.Id);
        if (company == null) return false;
        await _noSqlUnitOfWork.Companies.RemoveAsync(company.Id);
        await _cacheService.InvalidateCompaniesAsync(cancellationToken);
        return true;
    }
}

public sealed class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
