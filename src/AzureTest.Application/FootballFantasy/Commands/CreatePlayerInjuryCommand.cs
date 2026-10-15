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

public sealed record CreatePlayerInjuryCommand(
    Guid PlayerId,
    string Type,
    InjurySeverity Severity,
    string? Reason = null,
    DateTime? StartDate = null,
    DateTime? ExpectedReturn = null,
    Guid? FixtureId = null
) : ICommand<Result<Guid>>;

public sealed class CreatePlayerInjuryCommandValidator : AbstractValidator<CreatePlayerInjuryCommand>
{
    public CreatePlayerInjuryCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().MaximumLength(PlayerInjury.Rules.TypeMaxLength);
        RuleFor(x => x.Reason).MaximumLength(PlayerInjury.Rules.ReasonMaxLength);
    }
}

public sealed class CreatePlayerInjuryCommandHandler : BaseCommandHandler<CreatePlayerInjuryCommand, Result<Guid>>
{
    public CreatePlayerInjuryCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerInjuryCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerInjuryCommand request, CancellationToken cancellationToken)
    {
        var player = await UnitOfWork.Players.GetByIdAsync(request.PlayerId);
        if (player == null) return Result<Guid>.Failure("Player not found.");

        var injury = PlayerInjury.Create(
            request.PlayerId,
            request.Type,
            request.Severity,
            (request.StartDate ?? DateTime.UtcNow).ToUtc(),
            request.ExpectedReturn.ToUtc(),
            request.Reason,
            request.FixtureId
        );

        UnitOfWork.PlayerInjuries.Add(injury);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(injury.Id);
    }
}
