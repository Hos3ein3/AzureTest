using AzureTest.Application.Abstractions;

using FluentValidation;

using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(CreateCompanyDto Dto) : ICommand<Result<Guid>>;
public sealed class CreateCompanyCommandHandler : BaseCommandHandler<CreateCompanyCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly AzureTest.Application.Interfaces.Services.ICompanyCacheService _cacheService;
    public CreateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        AzureTest.Application.Interfaces.Services.ICompanyCacheService cacheService,
        ILogger<CreateCompanyCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cacheService = cacheService;
    }
    protected override async Task<Result<Guid>> ExecuteAsync(CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            CompanyName = command.Dto.CompanyName,
            KvkNumber = command.Dto.KvkNumber,
            IsINDMember = command.Dto.IsINDMember,
            Location = command.Dto.Location,
        };
        await _noSqlUnitOfWork.Companies.AddAsync(company);
        await _cacheService.InvalidateCompaniesAsync(cancellationToken);
        return company.Id;
    }
}

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {

    }
}
