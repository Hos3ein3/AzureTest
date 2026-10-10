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
    bool IsActive = true,
    decimal InitialPrice = 5.0m
) : ICommand<Result<Guid>>;

public sealed class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
{
    public CreatePlayerCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MinimumLength(Player.Rules.DisplayNameMinLength).MaximumLength(Player.Rules.DisplayNameMaxLength);
        RuleFor(x => x.FirstName).MaximumLength(Player.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).MaximumLength(Player.Rules.LastNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Player.Rules.PhotoUrlMaxLength);
    }
}

public sealed class CreatePlayerCommandHandler : BaseCommandHandler<CreatePlayerCommand, Result<Guid>>
{
    public CreatePlayerCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = Player.Create(
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.CountryId,
            request.Position,
            request.DateOfBirth,
            request.PreferredFoot,
            request.HeightCm,
            request.WeightKg,
            request.PhotoUrl,
            request.IsActive
        );
        UnitOfWork.Players.Add(player);

        // Add initial price
        var seasons = await UnitOfWork.Seasons.GetAllAsync();
        var currentSeason = seasons.FirstOrDefault(s => s.IsCurrent) ?? seasons.FirstOrDefault();
        if (currentSeason != null)
        {
            var price = PlayerPrice.Create(player.Id, currentSeason.Id, request.InitialPrice, DateTime.UtcNow, PlayerPriceReason.SeasonStart);
            UnitOfWork.PlayerPrices.Add(price);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(player.Id);
    }
}

public sealed record UpdatePlayerCommand(
    Guid Id,
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
    bool IsActive = true
) : ICommand<Result>;

public sealed class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MinimumLength(Player.Rules.DisplayNameMinLength).MaximumLength(Player.Rules.DisplayNameMaxLength);
        RuleFor(x => x.FirstName).MaximumLength(Player.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).MaximumLength(Player.Rules.LastNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Player.Rules.PhotoUrlMaxLength);
    }
}

public sealed class UpdatePlayerCommandHandler : BaseCommandHandler<UpdatePlayerCommand, Result>
{
    public UpdatePlayerCommandHandler(IUnitOfWork uow, ILogger<UpdatePlayerCommandHandler> logger) : base(uow, logger)
    {
    }

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
            request.DateOfBirth,
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

public sealed record DeletePlayerCommand(Guid Id) : ICommand<Result>;

public sealed class DeletePlayerCommandHandler : BaseCommandHandler<DeletePlayerCommand, Result>
{
    public DeletePlayerCommandHandler(IUnitOfWork uow, ILogger<DeletePlayerCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await UnitOfWork.Players.GetByIdAsync(request.Id);
        if (player == null) return Result.Failure("Player not found.");

        UnitOfWork.Players.Remove(player);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
