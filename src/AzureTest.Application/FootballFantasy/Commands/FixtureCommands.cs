using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
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
    int? RoundNumber = null,
    string? Venue = null,
    string? Referee = null
) : ICommand<Result<Guid>>;

public sealed class CreateFixtureCommandValidator : AbstractValidator<CreateFixtureCommand>
{
    public CreateFixtureCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty();
        RuleFor(x => x.HomeTeamId).NotEmpty();
        RuleFor(x => x.AwayTeamId).NotEmpty();
        RuleFor(x => x.HomeTeamId).NotEqual(x => x.AwayTeamId).WithMessage("Home team and Away team must be different.");
        RuleFor(x => x.Round).MaximumLength(Fixture.Rules.RoundMaxLength);
        RuleFor(x => x.Venue).MaximumLength(Fixture.Rules.VenueMaxLength);
        RuleFor(x => x.Referee).MaximumLength(Fixture.Rules.RefereeMaxLength);
    }
}

public sealed class CreateFixtureCommandHandler : BaseCommandHandler<CreateFixtureCommand, Result<Guid>>
{
    public CreateFixtureCommandHandler(IUnitOfWork uow, ILogger<CreateFixtureCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = Fixture.Create(
            request.SeasonId,
            request.HomeTeamId,
            request.AwayTeamId,
            request.KickoffAt,
            null,
            request.Round,
            request.RoundNumber,
            request.Venue,
            request.Referee
        );
        UnitOfWork.Fixtures.Add(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(fixture.Id);
    }
}

public sealed record UpdateFixtureScoreCommand(
    Guid Id,
    int HomeScore,
    int AwayScore,
    int? Minute = null,
    FixtureStatus Status = FixtureStatus.Finished,
    int? HomePen = null,
    int? AwayPen = null
) : ICommand<Result>;

public sealed class UpdateFixtureScoreCommandHandler : BaseCommandHandler<UpdateFixtureScoreCommand, Result>
{
    public UpdateFixtureScoreCommandHandler(IUnitOfWork uow, ILogger<UpdateFixtureScoreCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(UpdateFixtureScoreCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        fixture.UpdateScore(request.HomeScore, request.AwayScore, request.Minute, request.HomePen, request.AwayPen);
        fixture.UpdateStatus(request.Status);
        UnitOfWork.Fixtures.Update(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record UpdateFixtureCommand(
    Guid Id,
    DateTime KickoffAt,
    string? Round = null,
    int? RoundNumber = null,
    string? Venue = null,
    string? Referee = null,
    FixtureStatus Status = FixtureStatus.Scheduled
) : ICommand<Result>;

public sealed class UpdateFixtureCommandHandler : BaseCommandHandler<UpdateFixtureCommand, Result>
{
    public UpdateFixtureCommandHandler(IUnitOfWork uow, ILogger<UpdateFixtureCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(UpdateFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        fixture.UpdateDetails(request.KickoffAt, request.Round, request.RoundNumber, request.Venue, request.Referee);
        fixture.UpdateStatus(request.Status);
        UnitOfWork.Fixtures.Update(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DeleteFixtureCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteFixtureCommandHandler : BaseCommandHandler<DeleteFixtureCommand, Result>
{
    public DeleteFixtureCommandHandler(IUnitOfWork uow, ILogger<DeleteFixtureCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeleteFixtureCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        UnitOfWork.Fixtures.Remove(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
