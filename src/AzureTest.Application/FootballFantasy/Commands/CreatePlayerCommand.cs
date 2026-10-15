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

public sealed record CreatePlayerCommand(
    string FirstName,
    string LastName,
    string DisplayName,
    Guid CountryId,
    PlayerPosition Position,
    DateTime? DateOfBirth = null,
    PreferredFoot? PreferredFoot = null,
    int? HeightCm = null,
    int? WeightKg = null,
    string? PhotoUrl = null,
    decimal InitialPrice = 5.0m,
    bool IsActive = true
) : ICommand<Result<Guid>>;

public sealed class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
{
    public CreatePlayerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Player.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Player.Rules.LastNameMaxLength);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(Player.Rules.DisplayNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Player.Rules.PhotoUrlMaxLength);
    }
}

public sealed class CreatePlayerCommandHandler : BaseCommandHandler<CreatePlayerCommand, Result<Guid>>
{
    public CreatePlayerCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = Player.Create(
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

        UnitOfWork.Players.Add(player);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(player.Id);
    }
}
