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

public sealed record UpdateTeamCommand(
    Guid Id,
    string Name,
    string ShortName,
    TeamType TeamType,
    Guid CountryId,
    string? Code = null,
    int? FoundedYear = null,
    string? StadiumName = null,
    int? StadiumCapacity = null,
    string? LogoUrl = null,
    Guid? VenueId = null
) : ICommand<Result>;

public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Team.Rules.NameMinLength).MaximumLength(Team.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(Team.Rules.ShortNameMaxLength);
        RuleFor(x => x.Code).MaximumLength(Team.Rules.CodeMaxLength);
        RuleFor(x => x.StadiumName).MaximumLength(Team.Rules.StadiumNameMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(Team.Rules.LogoUrlMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
    }
}

public sealed class UpdateTeamCommandHandler : BaseCommandHandler<UpdateTeamCommand, Result>
{
    public UpdateTeamCommandHandler(IUnitOfWork uow, ILogger<UpdateTeamCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await UnitOfWork.Teams.GetByIdAsync(request.Id);
        if (team == null) return Result.Failure("Team not found.");

        team.Update(
            request.Name,
            request.ShortName,
            request.TeamType,
            request.CountryId,
            request.Code,
            request.FoundedYear,
            request.StadiumName,
            request.StadiumCapacity,
            request.LogoUrl,
            request.VenueId
        );

        UnitOfWork.Teams.Update(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
