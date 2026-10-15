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

public sealed record UpdateCountryCommand(Guid Id, string Name, string Continent, string? FlagUrl) : ICommand<Result>;

public sealed class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Country.Rules.NameMinLength).MaximumLength(Country.Rules.NameMaxLength);
        RuleFor(x => x.Continent).NotEmpty().MaximumLength(Country.Rules.ContinentMaxLength);
        RuleFor(x => x.FlagUrl).MaximumLength(Country.Rules.FlagUrlMaxLength);
    }
}

public sealed class UpdateCountryCommandHandler : BaseCommandHandler<UpdateCountryCommand, Result>
{
    public UpdateCountryCommandHandler(IUnitOfWork uow, ILogger<UpdateCountryCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await UnitOfWork.Countries.GetByIdAsync(request.Id);
        if (country == null) return Result.Failure("Country not found.");

        country.Update(request.Name, request.Continent, request.FlagUrl);
        UnitOfWork.Countries.Update(country);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
