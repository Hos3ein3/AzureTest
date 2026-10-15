using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreateLeagueCommand(string Name, LeagueType Type, LeagueLevel Level, string? ShortName = null, Guid? CountryId = null, string? Continent = null, string? LogoUrl = null, bool IsActive = true) : ICommand<Result<Guid>>;

public sealed class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
{
    public CreateLeagueCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(League.Rules.NameMinLength).MaximumLength(League.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).MaximumLength(League.Rules.ShortNameMaxLength);
        RuleFor(x => x.Continent).MaximumLength(League.Rules.ContinentMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(League.Rules.LogoUrlMaxLength);
    }
}

public sealed class CreateLeagueCommandHandler : BaseCommandHandler<CreateLeagueCommand, Result<Guid>>
{
    public CreateLeagueCommandHandler(IUnitOfWork uow, ILogger<CreateLeagueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = League.Create(request.Name, request.Type, request.Level, request.ShortName, request.CountryId, request.Continent, request.LogoUrl, request.IsActive);
        UnitOfWork.Leagues.Add(league);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(league.Id);
    }
}
