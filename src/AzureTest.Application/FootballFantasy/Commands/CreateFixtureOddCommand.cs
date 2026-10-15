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

public sealed record CreateFixtureOddCommand(
    Guid FixtureId,
    Guid BookmakerId,
    OddsMarketType MarketType,
    string ValueLabel,
    decimal Price,
    bool IsLive = false
) : ICommand<Result<Guid>>;

public sealed class CreateFixtureOddCommandValidator : AbstractValidator<CreateFixtureOddCommand>
{
    public CreateFixtureOddCommandValidator()
    {
        RuleFor(x => x.FixtureId).NotEmpty();
        RuleFor(x => x.BookmakerId).NotEmpty();
        RuleFor(x => x.ValueLabel).NotEmpty().MaximumLength(FixtureOdd.Rules.ValueLabelMaxLength);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(1.00m);
    }
}

public sealed class CreateFixtureOddCommandHandler : BaseCommandHandler<CreateFixtureOddCommand, Result<Guid>>
{
    public CreateFixtureOddCommandHandler(IUnitOfWork uow, ILogger<CreateFixtureOddCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateFixtureOddCommand request, CancellationToken cancellationToken)
    {
        var fixture = await UnitOfWork.Fixtures.GetByIdAsync(request.FixtureId);
        if (fixture == null) return Result<Guid>.Failure("Fixture not found.");

        var bookmaker = await UnitOfWork.Bookmakers.GetByIdAsync(request.BookmakerId);
        if (bookmaker == null) return Result<Guid>.Failure("Bookmaker not found.");

        var odd = FixtureOdd.Create(
            request.FixtureId,
            request.BookmakerId,
            request.MarketType,
            request.ValueLabel,
            request.Price,
            request.IsLive
        );

        UnitOfWork.FixtureOdds.Add(odd);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(odd.Id);
    }
}
