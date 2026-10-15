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

public sealed record CreateFixtureCommand(
    Guid SeasonId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    DateTime KickoffAt,
    string? Round = null,
    string? Venue = null
) : ICommand<Result<Guid>>;

public sealed class CreateFixtureCommandValidator : AbstractValidator<CreateFixtureCommand>
{
    public CreateFixtureCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty();
        RuleFor(x => x.HomeTeamId).NotEmpty();
        RuleFor(x => x.AwayTeamId).NotEmpty().NotEqual(x => x.HomeTeamId).WithMessage("Home and Away teams must be different.");
        RuleFor(x => x.Round).MaximumLength(Fixture.Rules.RoundMaxLength);
        RuleFor(x => x.Venue).MaximumLength(Fixture.Rules.VenueMaxLength);
    }
}

public sealed class CreateFixtureCommandHandler : BaseCommandHandler<CreateFixtureCommand, Result<Guid>>
{
    public CreateFixtureCommandHandler(IUnitOfWork uow, ILogger<CreateFixtureCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = Fixture.Create(request.SeasonId, request.HomeTeamId, request.AwayTeamId, request.KickoffAt.ToUtc(), null, request.Round, null, request.Venue);
        UnitOfWork.Fixtures.Add(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(fixture.Id);
    }
}
