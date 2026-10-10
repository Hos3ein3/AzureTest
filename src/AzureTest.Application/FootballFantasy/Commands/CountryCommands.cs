using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
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
    public CreateCountryCommandHandler(IUnitOfWork uow, ILogger<CreateCountryCommandHandler> logger) : base(uow, logger)
    {
    }

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
    public UpdateCountryCommandHandler(IUnitOfWork uow, ILogger<UpdateCountryCommandHandler> logger) : base(uow, logger)
    {
    }

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

public sealed record DeleteCountryCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteCountryCommandHandler : BaseCommandHandler<DeleteCountryCommand, Result>
{
    public DeleteCountryCommandHandler(IUnitOfWork uow, ILogger<DeleteCountryCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await UnitOfWork.Countries.GetByIdAsync(request.Id);
        if (country == null) return Result.Failure("Country not found.");

        UnitOfWork.Countries.Remove(country);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
