using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.CloseCryptoPosition;

public sealed record CloseCryptoPositionCommand(Guid PositionId, decimal? ClosePrice = null) : ICommand<Result<decimal>>;

public sealed class CloseCryptoPositionCommandValidator : AbstractValidator<CloseCryptoPositionCommand>
{
    public CloseCryptoPositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public sealed class CloseCryptoPositionCommandHandler : ICommandHandler<CloseCryptoPositionCommand, Result<decimal>>
{
    private readonly ICryptoExecutionService _executionService;

    public CloseCryptoPositionCommandHandler(ICryptoExecutionService executionService)
    {
        _executionService = executionService;
    }

    public async Task<Result<decimal>> HandleAsync(CloseCryptoPositionCommand cmd, CancellationToken ct)
    {
        return await _executionService.ClosePositionAsync(cmd.PositionId, cmd.ClosePrice, ct);
    }
}
