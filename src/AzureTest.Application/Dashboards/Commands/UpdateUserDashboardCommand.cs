using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.Dashboards.Commands;

public sealed record UpdateUserDashboardCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string Slug,
    List<DashboardSectionDto> Sections) : ICommand;

public sealed class UpdateUserDashboardCommandHandler : ICommandHandler<UpdateUserDashboardCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public UpdateUserDashboardCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task HandleAsync(UpdateUserDashboardCommand request, CancellationToken cancellationToken)
    {
        var dashboard = await _noSqlUnitOfWork.UserDashboards.GetByIdAsync(request.Id, cancellationToken);
        if (dashboard == null || dashboard.UserId != request.UserId)
        {
            throw new Exception("Dashboard not found or access denied.");
        }

        dashboard.Name = request.Name;
        dashboard.Slug = request.Slug;
        dashboard.Sections = request.Sections.Select(s => new DashboardSection
        {
            Id = s.Id,
            Name = s.Name,
            Components = s.Components.Select(c => new UserDashboardComponent
            {
                ComponentId = c.ComponentId,
                X = c.X,
                Y = c.Y,
                Width = c.Width,
                Height = c.Height
            }).ToList()
        }).ToList();

        await _noSqlUnitOfWork.UserDashboards.UpdateAsync(dashboard.Id,dashboard, cancellationToken);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class UpdateUserDashboardCommandValidator : AbstractValidator<UpdateUserDashboardCommand>
{
    public UpdateUserDashboardCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
    }
}
