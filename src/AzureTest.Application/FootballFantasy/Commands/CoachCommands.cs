using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record CreateCoachCommand(
    string FirstName,
    string LastName,
    Guid CountryId,
    DateTime? DateOfBirth = null,
    string? PhotoUrl = null
) : ICommand<Result<Guid>>;

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
    public CreateCoachCommandHandler(IUnitOfWork uow, ILogger<CreateCoachCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = Coach.Create(request.FirstName, request.LastName, request.CountryId, request.DateOfBirth, request.PhotoUrl);
        UnitOfWork.Coaches.Add(coach);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(coach.Id);
    }
}

public sealed record UpdateCoachCommand(
    Guid Id,
    string FirstName,
    string LastName,
    Guid CountryId,
    DateTime? DateOfBirth = null,
    string? PhotoUrl = null
) : ICommand<Result>;

public sealed class UpdateCoachCommandValidator : AbstractValidator<UpdateCoachCommand>
{
    public UpdateCoachCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(Coach.Rules.FirstNameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(Coach.Rules.LastNameMaxLength);
        RuleFor(x => x.CountryId).NotEmpty();
        RuleFor(x => x.PhotoUrl).MaximumLength(Coach.Rules.PhotoUrlMaxLength);
    }
}

public sealed class UpdateCoachCommandHandler : BaseCommandHandler<UpdateCoachCommand, Result>
{
    public UpdateCoachCommandHandler(IUnitOfWork uow, ILogger<UpdateCoachCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(UpdateCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = await UnitOfWork.Coaches.GetByIdAsync(request.Id);
        if (coach == null) return Result.Failure("Coach not found.");

        coach.Update(request.FirstName, request.LastName, request.CountryId, request.DateOfBirth, request.PhotoUrl);
        UnitOfWork.Coaches.Update(coach);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record DeleteCoachCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteCoachCommandHandler : BaseCommandHandler<DeleteCoachCommand, Result>
{
    public DeleteCoachCommandHandler(IUnitOfWork uow, ILogger<DeleteCoachCommandHandler> logger) : base(uow, logger)
    {
    }

    protected override async Task<Result> ExecuteAsync(DeleteCoachCommand request, CancellationToken cancellationToken)
    {
        var coach = await UnitOfWork.Coaches.GetByIdAsync(request.Id);
        if (coach == null) return Result.Failure("Coach not found.");

        UnitOfWork.Coaches.Remove(coach);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
