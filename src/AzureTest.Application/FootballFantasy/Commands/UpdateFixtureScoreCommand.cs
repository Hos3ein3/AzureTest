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

public sealed record UpdateFixtureScoreCommand(Guid Id, int HomeScore, int AwayScore, FixtureStatus Status, int? Minute = null) : ICommand<Result>;

public sealed class UpdateFixtureScoreCommandValidator : AbstractValidator<UpdateFixtureScoreCommand>
{
    public UpdateFixtureScoreCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.HomeScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AwayScore).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateFixtureScoreCommandHandler : BaseCommandHandler<UpdateFixtureScoreCommand, Result>
{
    public UpdateFixtureScoreCommandHandler(IUnitOfWork uow, ILogger<UpdateFixtureScoreCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateFixtureScoreCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.Id);
        if (fixture == null) return Result.Failure("Fixture not found.");

        fixture.UpdateScore(request.HomeScore, request.AwayScore, request.Minute);
        fixture.UpdateStatus(request.Status);
        UnitOfWork.Fixtures.Update(fixture);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
