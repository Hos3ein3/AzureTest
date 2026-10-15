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

public sealed record CreatePlayerTransferCommand(
    Guid PlayerId,
    DateTime TransferDate,
    TransferType Type,
    Guid? FromTeamId,
    Guid? ToTeamId,
    string? Fee
) : ICommand<Result<Guid>>;

public sealed class CreatePlayerTransferCommandValidator : AbstractValidator<CreatePlayerTransferCommand>
{
    public CreatePlayerTransferCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Fee).MaximumLength(PlayerTransfer.Rules.FeeMaxLength);
    }
}

public sealed class CreatePlayerTransferCommandHandler : BaseCommandHandler<CreatePlayerTransferCommand, Result<Guid>>
{
    public CreatePlayerTransferCommandHandler(IUnitOfWork uow, ILogger<CreatePlayerTransferCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreatePlayerTransferCommand request, CancellationToken cancellationToken)
    {
        var player = await UnitOfWork.Players.GetByIdAsync(request.PlayerId);
        if (player == null) return Result<Guid>.Failure("Player not found.");

        var transfer = PlayerTransfer.Create(
            request.PlayerId,
            request.TransferDate.ToUtc(),
            request.Type,
            request.FromTeamId,
            request.ToTeamId,
            request.Fee
        );

        UnitOfWork.PlayerTransfers.Add(transfer);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(transfer.Id);
    }
}
