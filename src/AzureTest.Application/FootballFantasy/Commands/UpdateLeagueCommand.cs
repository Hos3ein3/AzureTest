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

public sealed record UpdateLeagueCommand(Guid Id, string Name, LeagueType Type, LeagueLevel Level, string? ShortName = null, Guid? CountryId = null, string? Continent = null, string? LogoUrl = null, bool IsActive = true) : ICommand<Result>;

public sealed class UpdateLeagueCommandValidator : AbstractValidator<UpdateLeagueCommand>
{
    public UpdateLeagueCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(League.Rules.NameMinLength).MaximumLength(League.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).MaximumLength(League.Rules.ShortNameMaxLength);
        RuleFor(x => x.Continent).MaximumLength(League.Rules.ContinentMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(League.Rules.LogoUrlMaxLength);
    }
}

public sealed class UpdateLeagueCommandHandler : BaseCommandHandler<UpdateLeagueCommand, Result>
{
    public UpdateLeagueCommandHandler(IUnitOfWork uow, ILogger<UpdateLeagueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = await UnitOfWork.Leagues.GetByIdAsync(request.Id);
        if (league == null) return Result.Failure("League not found.");

        league.Update(request.Name, request.ShortName, request.Type, request.Level, request.CountryId, request.Continent, request.LogoUrl, request.IsActive);
        UnitOfWork.Leagues.Update(league);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
