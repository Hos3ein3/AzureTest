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

public sealed record CreateTeamCommand(
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
) : ICommand<Result<Guid>>;

public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Team.Rules.NameMinLength).MaximumLength(Team.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(Team.Rules.ShortNameMaxLength);
        RuleFor(x => x.Code).MaximumLength(Team.Rules.CodeMaxLength);
        RuleFor(x => x.StadiumName).MaximumLength(Team.Rules.StadiumNameMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(Team.Rules.LogoUrlMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
    }
}

public sealed class CreateTeamCommandHandler : BaseCommandHandler<CreateTeamCommand, Result<Guid>>
{
    public CreateTeamCommandHandler(IUnitOfWork uow, ILogger<CreateTeamCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = Team.Create(
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

        UnitOfWork.Teams.Add(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(team.Id);
    }
}
