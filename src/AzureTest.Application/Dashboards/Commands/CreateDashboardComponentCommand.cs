using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.Dashboards.Commands;

public sealed record CreateDashboardComponentCommand(
    string Name,
    string Description,
    string ImagePath,
    string Category,
    string RequiredRole,
    string PartialViewName,
    int DefaultWidth,
    int DefaultHeight) : ICommand;

public sealed class CreateDashboardComponentCommandHandler : ICommandHandler<CreateDashboardComponentCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public CreateDashboardComponentCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task HandleAsync(CreateDashboardComponentCommand request, CancellationToken cancellationToken)
    {
        var component = new DashboardComponent
        {
            Name = request.Name,
            Description = request.Description,
            ImagePath = request.ImagePath,
            Category = request.Category,
            RequiredRole = request.RequiredRole,
            PartialViewName = request.PartialViewName,
            DefaultWidth = request.DefaultWidth,
            DefaultHeight = request.DefaultHeight
        };

        await _noSqlUnitOfWork.DashboardComponents.AddAsync(component, cancellationToken);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class CreateDashboardComponentCommandValidator : AbstractValidator<CreateDashboardComponentCommand>
{
    public CreateDashboardComponentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RequiredRole).MaximumLength(50);
        RuleFor(x => x.PartialViewName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DefaultWidth).GreaterThan(0);
        RuleFor(x => x.DefaultHeight).GreaterThan(0);
    }
}
