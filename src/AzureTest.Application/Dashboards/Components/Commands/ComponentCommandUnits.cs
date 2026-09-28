using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.Dashboards.Components.Commands;

// CREATE
public record CreateDashboardComponentCommand(
    string Name,
    string Description,
    string ImagePath,
    string Category,
    string RequiredRole,
    string PartialViewName,
    int DefaultWidth,
    int DefaultHeight
) : ICommand;

public class CreateDashboardComponentCommandValidator : AbstractValidator<CreateDashboardComponentCommand>
{
    public CreateDashboardComponentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PartialViewName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DefaultWidth).GreaterThan(0);
        RuleFor(x => x.DefaultHeight).GreaterThan(0);
    }
}

public class CreateDashboardComponentCommandHandler : ICommandHandler<CreateDashboardComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public CreateDashboardComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreateDashboardComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = new DashboardComponent
        {
            Name = command.Name,
            Description = command.Description ?? "",
            ImagePath = command.ImagePath ?? "",
            Category = command.Category,
            RequiredRole = command.RequiredRole ?? "",
            PartialViewName = command.PartialViewName,
            DefaultWidth = command.DefaultWidth,
            DefaultHeight = command.DefaultHeight
        };

        await _unitOfWork.DashboardComponents.AddAsync(component, cancellationToken);
    }
}

// UPDATE
public record UpdateDashboardComponentCommand(
    Guid Id,
    string Name,
    string Description,
    string ImagePath,
    string Category,
    string RequiredRole,
    string PartialViewName,
    int DefaultWidth,
    int DefaultHeight
) : ICommand;

public class UpdateDashboardComponentCommandValidator : AbstractValidator<UpdateDashboardComponentCommand>
{
    public UpdateDashboardComponentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PartialViewName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DefaultWidth).GreaterThan(0);
        RuleFor(x => x.DefaultHeight).GreaterThan(0);
    }
}

public class UpdateDashboardComponentCommandHandler : ICommandHandler<UpdateDashboardComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public UpdateDashboardComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(UpdateDashboardComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = await _unitOfWork.DashboardComponents.GetByIdAsync(command.Id, cancellationToken);
        if (component == null) return;

        component.Name = command.Name;
        component.Description = command.Description ?? "";
        component.ImagePath = command.ImagePath ?? "";
        component.Category = command.Category;
        component.RequiredRole = command.RequiredRole ?? "";
        component.PartialViewName = command.PartialViewName;
        component.DefaultWidth = command.DefaultWidth;
        component.DefaultHeight = command.DefaultHeight;

        await _unitOfWork.DashboardComponents.UpdateAsync(component.Id,component, cancellationToken);
    }
}

// DELETE
public record DeleteDashboardComponentCommand(Guid Id) : ICommand;

public class DeleteDashboardComponentCommandValidator : AbstractValidator<DeleteDashboardComponentCommand>
{
    public DeleteDashboardComponentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteDashboardComponentCommandHandler : ICommandHandler<DeleteDashboardComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public DeleteDashboardComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteDashboardComponentCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.DashboardComponents.RemoveAsync(command.Id, cancellationToken);
    }
}
