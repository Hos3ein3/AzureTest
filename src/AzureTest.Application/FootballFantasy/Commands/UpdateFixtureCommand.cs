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

public sealed record UpdateFixtureCommand(
    Guid Id,
    DateTime KickoffAt,
    string? Round = null,
    int? RoundNumber = null,
    string? Venue = null,
    string? Referee = null,
    FixtureStatus? Status = null
) : ICommand<Result>;

public sealed class UpdateFixtureCommandValidator : AbstractValidator<UpdateFixtureCommand>
{
    public UpdateFixtureCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Round).MaximumLength(Fixture.Rules.RoundMaxLength);
        RuleFor(x => x.Venue).MaximumLength(Fixture.Rules.VenueMaxLength);
        RuleFor(x => x.Referee).MaximumLength(Fixture.Rules.RefereeMaxLength);
    }
}

public sealed class UpdateFixtureCommandHandler : BaseCommandHandler<UpdateFixtureCommand, Result>
{
    public UpdateFixtureCommandHandler(IUnitOfWork uow, ILogger<UpdateFixtureCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        fixture.UpdateDetails(request.KickoffAt.ToUtc(), request.Round, request.RoundNumber, request.Venue);
        UnitOfWork.Fixtures.Update(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
