using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.UpdateCryptoStops;

public sealed record UpdateCryptoStopsCommand(
    Guid PositionId,
    decimal? StopLoss,
    decimal? TakeProfit,
    decimal? TrailingStopDistance) : ICommand<Result>;

public sealed class UpdateCryptoStopsCommandValidator : AbstractValidator<UpdateCryptoStopsCommand>
{
    public UpdateCryptoStopsCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public sealed class UpdateCryptoStopsCommandHandler : ICommandHandler<UpdateCryptoStopsCommand, Result>
{
    private readonly ICryptoExecutionService _executionService;

    public UpdateCryptoStopsCommandHandler(ICryptoExecutionService executionService)
    {
        _executionService = executionService;
    }

    public async Task<Result> HandleAsync(UpdateCryptoStopsCommand cmd, CancellationToken ct)
    {
        return await _executionService.UpdateStopsAsync(
            cmd.PositionId,
            cmd.StopLoss,
            cmd.TakeProfit,
            cmd.TrailingStopDistance,
            ct);
    }
}
