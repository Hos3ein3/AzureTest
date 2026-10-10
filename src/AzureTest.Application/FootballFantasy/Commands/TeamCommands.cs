using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
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
    string? LogoUrl = null
) : ICommand<Result<Guid>>;

public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Team.Rules.NameMinLength).MaximumLength(Team.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(Team.Rules.ShortNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.Code).MaximumLength(Team.Rules.CodeMaxLength);
        RuleFor(x => x.StadiumName).MaximumLength(Team.Rules.StadiumNameMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(Team.Rules.LogoUrlMaxLength);
    }
}

public sealed class CreateTeamCommandHandler : BaseCommandHandler<CreateTeamCommand, Result<Guid>>
{
    public CreateTeamCommandHandler(IUnitOfWork uow, ILogger<CreateTeamCommandHandler> logger) : base(uow, logger)
    {
    }

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
            request.LogoUrl
        );
        UnitOfWork.Teams.Add(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(team.Id);
    }
}

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
    string? LogoUrl = null
) : ICommand<Result>;

public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(Team.Rules.NameMinLength).MaximumLength(Team.Rules.NameMaxLength);
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(Team.Rules.ShortNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.Code).MaximumLength(Team.Rules.CodeMaxLength);
        RuleFor(x => x.StadiumName).MaximumLength(Team.Rules.StadiumNameMaxLength);
        RuleFor(x => x.LogoUrl).MaximumLength(Team.Rules.LogoUrlMaxLength);
    }
}

public sealed class UpdateTeamCommandHandler : BaseCommandHandler<UpdateTeamCommand, Result>
{
    public UpdateTeamCommandHandler(IUnitOfWork uow, ILogger<UpdateTeamCommandHandler> logger) : base(uow, logger)
    {
    }

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
            request.LogoUrl
        );
        UnitOfWork.Teams.Update(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DeleteTeamCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteTeamCommandHandler : BaseCommandHandler<DeleteTeamCommand, Result>
{
    public DeleteTeamCommandHandler(IUnitOfWork uow, ILogger<DeleteTeamCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await UnitOfWork.Teams.GetByIdAsync(request.Id);
        if (team == null) return Result.Failure("Team not found.");

        UnitOfWork.Teams.Remove(team);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
