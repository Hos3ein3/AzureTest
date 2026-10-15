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

public sealed record UpdatePlayerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    Guid CountryId,
    PlayerPosition Position,
    DateTime? DateOfBirth,
    PreferredFoot? PreferredFoot,
    int? HeightCm,
    int? WeightKg,
    string? PhotoUrl,
    bool IsActive
) : ICommand<Result>;

public sealed class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Player.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Player.Rules.LastNameMaxLength);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(Player.Rules.DisplayNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Player.Rules.PhotoUrlMaxLength);
    }
}

public sealed class UpdatePlayerCommandHandler : BaseCommandHandler<UpdatePlayerCommand, Result>
{
    public UpdatePlayerCommandHandler(IUnitOfWork uow, ILogger<UpdatePlayerCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await UnitOfWork.Players.GetByIdAsync(request.Id);
        if (player == null) return Result.Failure("Player not found.");

        player.Update(
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.CountryId,
            request.Position,
            request.DateOfBirth.ToUtc(),
            request.PreferredFoot,
            request.HeightCm,
            request.WeightKg,
            request.PhotoUrl,
            request.IsActive
        );

        UnitOfWork.Players.Update(player);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
