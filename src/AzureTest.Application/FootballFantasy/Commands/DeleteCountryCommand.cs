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

public sealed record DeleteCountryCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteCountryCommandHandler : BaseCommandHandler<DeleteCountryCommand, Result>
{
    public DeleteCountryCommandHandler(IUnitOfWork uow, ILogger<DeleteCountryCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await UnitOfWork.Countries.GetByIdAsync(request.Id);
        if (country == null) return Result.Failure("Country not found.");

        UnitOfWork.Countries.Remove(country);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
