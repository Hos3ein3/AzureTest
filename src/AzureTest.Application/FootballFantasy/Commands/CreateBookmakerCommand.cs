using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreateBookmakerCommand(string Name, string? Url) : ICommand<Result<Guid>>;

public sealed class CreateBookmakerCommandValidator : AbstractValidator<CreateBookmakerCommand>
{
    public CreateBookmakerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Bookmaker.Rules.NameMaxLength);

        RuleFor(x => x.Url)
            .MaximumLength(Bookmaker.Rules.UrlMaxLength);
    }
}

public sealed class CreateBookmakerCommandHandler : BaseCommandHandler<CreateBookmakerCommand, Result<Guid>>
{
    public CreateBookmakerCommandHandler(IUnitOfWork uow, ILogger<CreateBookmakerCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateBookmakerCommand request, CancellationToken cancellationToken)
    {
        var bookmaker = Bookmaker.Create(request.Name, request.Url);
        UnitOfWork.Bookmakers.Add(bookmaker);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(bookmaker.Id);
    }
}
