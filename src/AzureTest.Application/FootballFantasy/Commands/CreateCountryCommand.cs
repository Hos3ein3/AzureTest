using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreateCountryCommand(string Name, string Code, string Continent, string? FlagUrl) : ICommand<Result<Guid>>;

public sealed class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Country.Rules.NameMinLength).MaximumLength(Country.Rules.NameMaxLength);
        RuleFor(x => x.Code).NotEmpty().MinimumLength(Country.Rules.CodeMinLength).MaximumLength(Country.Rules.CodeMaxLength);
        RuleFor(x => x.Continent).NotEmpty().MaximumLength(Country.Rules.ContinentMaxLength);
        RuleFor(x => x.FlagUrl).MaximumLength(Country.Rules.FlagUrlMaxLength);
    }
}

public sealed class CreateCountryCommandHandler : BaseCommandHandler<CreateCountryCommand, Result<Guid>>
{
    public CreateCountryCommandHandler(IUnitOfWork uow, ILogger<CreateCountryCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        var existing = await UnitOfWork.Countries.FindAsync(c => c.Code.ToUpper() == request.Code.Trim().ToUpper());
        if (existing.Count > 0)
            return Result<Guid>.Failure($"Country with code '{request.Code}' already exists.");

        var country = Country.Create(request.Name, request.Code, request.Continent, request.FlagUrl);
        UnitOfWork.Countries.Add(country);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(country.Id);
    }
}
