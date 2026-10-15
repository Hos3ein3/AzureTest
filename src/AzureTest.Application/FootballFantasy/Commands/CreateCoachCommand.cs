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

public sealed record CreateCoachCommand(string FirstName, string LastName, Guid CountryId, DateTime? DateOfBirth = null, string? PhotoUrl = null) : ICommand<Result<Guid>>;

public sealed class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
{
    public CreateCoachCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Coach.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Coach.Rules.LastNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Coach.Rules.PhotoUrlMaxLength);
    }
}

public sealed class CreateCoachCommandHandler : BaseCommandHandler<CreateCoachCommand, Result<Guid>>
{
    public CreateCoachCommandHandler(IUnitOfWork uow, ILogger<CreateCoachCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = Coach.Create(request.FirstName, request.LastName, request.CountryId, request.DateOfBirth.ToUtc(), request.PhotoUrl);
        UnitOfWork.Coaches.Add(coach);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(coach.Id);
    }
}
