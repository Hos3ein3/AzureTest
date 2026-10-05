using AzureTest.Application.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.Dashboards.RawComponents;

// ---------------------------------------------------------
// QUERIES
// ---------------------------------------------------------

public record GetAllRawComponentsQuery() : IQuery<IReadOnlyList<DashboardRawComponent>>;

public class GetAllRawComponentsQueryHandler : IQueryHandler<GetAllRawComponentsQuery, IReadOnlyList<DashboardRawComponent>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetAllRawComponentsQueryHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DashboardRawComponent>> HandleAsync(GetAllRawComponentsQuery query, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.DashboardRawComponents.GetAllAsync(cancellationToken);
    }
}



public record GetRawComponentByIdQuery(Guid Id) : IQuery<DashboardRawComponent?>;

public class GetRawComponentByIdQueryHandler : IQueryHandler<GetRawComponentByIdQuery, DashboardRawComponent?>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetRawComponentByIdQueryHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardRawComponent?> HandleAsync(GetRawComponentByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.DashboardRawComponents.GetByIdAsync(query.Id, cancellationToken);
    }
}

// ---------------------------------------------------------
// COMMANDS
// ---------------------------------------------------------

public record CreateRawComponentCommand(
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? ViewName,
    string? Category,
    string? RequiredRole,
    int MinWidth,
    int MaxWidth,
    int MinHeight,
    int MaxHeight,
    bool IsResizable
) : ICommand<Result<Guid>>;

public class CreateRawComponentCommandValidator : AbstractValidator<CreateRawComponentCommand>
{
    public CreateRawComponentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).Must(t => t == "RawHtml" || t == "JsonConfig").WithMessage("Type must be RawHtml or JsonConfig");
        
        RuleFor(x => x.HtmlContent)
            .NotEmpty()
            .When(x => x.Type == "RawHtml")
            .WithMessage("HtmlContent is required for RawHtml type.");
            
        RuleFor(x => x.JsonConfiguration)
            .NotEmpty()
            .When(x => x.Type == "JsonConfig")
            .WithMessage("JsonConfiguration is required for JsonConfig type.");
            
        RuleFor(x => x.MinWidth).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxWidth).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinHeight).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxHeight).GreaterThanOrEqualTo(0);
    }
}

public class CreateRawComponentCommandHandler : ICommandHandler<CreateRawComponentCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public CreateRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(CreateRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = new DashboardRawComponent
        {
            Name = command.Name,
            Type = command.Type,
            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            ViewName = command.ViewName,
            Category = command.Category,
            RequiredRole = command.RequiredRole ?? string.Empty,
            MinWidth = command.MinWidth,
            MaxWidth = command.MaxWidth,
            MinHeight = command.MinHeight,
            MaxHeight = command.MaxHeight,
            IsResizable = command.IsResizable
        };

        await _unitOfWork.DashboardRawComponents.AddAsync(component, cancellationToken);
        return Result<Guid>.Success(component.Id);
    }
}

public record DeleteRawComponentCommand(Guid Id)  : ICommand;

public class DeleteRawComponentCommandHandler : ICommandHandler<DeleteRawComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public DeleteRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(DeleteRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.DashboardRawComponents.RemoveAsync(command.Id, cancellationToken);
        return Result.Success();
    }
}

public record UpdateRawComponentCommand(
    Guid Id,
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? ViewName,
    string? Category,
    string? RequiredRole,
    int MinWidth,
    int MaxWidth,
    int MinHeight,
    int MaxHeight,
    bool IsResizable
)  : ICommand;

public class UpdateRawComponentCommandValidator : AbstractValidator<UpdateRawComponentCommand>
{
    public UpdateRawComponentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).Must(t => t == "RawHtml" || t == "JsonConfig").WithMessage("Type must be RawHtml or JsonConfig");
        
        RuleFor(x => x.HtmlContent)
            .NotEmpty()
            .When(x => x.Type == "RawHtml")
            .WithMessage("HtmlContent is required for RawHtml type.");
            
        RuleFor(x => x.JsonConfiguration)
            .NotEmpty()
            .When(x => x.Type == "JsonConfig")
            .WithMessage("JsonConfiguration is required for JsonConfig type.");
            
        RuleFor(x => x.MinWidth).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxWidth).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinHeight).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxHeight).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRawComponentCommandHandler : ICommandHandler<UpdateRawComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public UpdateRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = await _unitOfWork.DashboardRawComponents.GetByIdAsync(command.Id, cancellationToken);
        if (component == null) return Result.Failure("Not found");
        
        component.Name = command.Name;
        component.Type = command.Type;
        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.ViewName = command.ViewName;
        component.Category = command.Category;
        component.RequiredRole = command.RequiredRole ?? string.Empty;
        component.MinWidth = command.MinWidth;
        component.MaxWidth = command.MaxWidth;
        component.MinHeight = command.MinHeight;
        component.MaxHeight = command.MaxHeight;
        component.IsResizable = command.IsResizable;
        
        await _unitOfWork.DashboardRawComponents.UpdateAsync(component.Id, component, cancellationToken);
        return Result.Success();
    }
}
