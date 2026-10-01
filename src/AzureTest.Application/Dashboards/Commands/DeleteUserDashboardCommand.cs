using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Dashboards.Commands;

public sealed record DeleteUserDashboardCommand(Guid Id, Guid UserId) : ICommand;

public sealed class DeleteUserDashboardCommandHandler : ICommandHandler<DeleteUserDashboardCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public DeleteUserDashboardCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<Result> HandleAsync(DeleteUserDashboardCommand request, CancellationToken cancellationToken)
    {
        var dashboard = await _noSqlUnitOfWork.UserDashboards.GetByIdAsync(request.Id, cancellationToken);
        if (dashboard == null || dashboard.UserId != request.UserId)
        {
            throw new Exception("Dashboard not found or access denied.");
        return Result.Success();
        }

        await _noSqlUnitOfWork.UserDashboards.RemoveAsync(request.Id, cancellationToken);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteUserDashboardCommandValidator : AbstractValidator<DeleteUserDashboardCommand>
{
    public DeleteUserDashboardCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
