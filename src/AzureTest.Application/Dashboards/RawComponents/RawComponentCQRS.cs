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
    string? Category,
    string RequiredRole
) : ICommand;

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
    }
}

public class CreateRawComponentCommandHandler : ICommandHandler<CreateRawComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public CreateRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreateRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = new DashboardRawComponent
        {
            Name = command.Name,
            Type = command.Type,
            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            Category = command.Category,
            RequiredRole = command.RequiredRole ?? string.Empty
        };

        await _unitOfWork.DashboardRawComponents.AddAsync(component, cancellationToken);
    }
}

public record DeleteRawComponentCommand(Guid Id) : ICommand;

public class DeleteRawComponentCommandHandler : ICommandHandler<DeleteRawComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public DeleteRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.DashboardRawComponents.RemoveAsync(command.Id, cancellationToken);
    }
}

public record UpdateRawComponentCommand(
    Guid Id,
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? Category,
    string RequiredRole
) : ICommand;

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
    }
}

public class UpdateRawComponentCommandHandler : ICommandHandler<UpdateRawComponentCommand>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public UpdateRawComponentCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(UpdateRawComponentCommand command, CancellationToken cancellationToken = default)
    {
        var component = await _unitOfWork.DashboardRawComponents.GetByIdAsync(command.Id, cancellationToken);
        if (component == null) return;
        
        component.Name = command.Name;
        component.Type = command.Type;
        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.Category = command.Category;
        component.RequiredRole = command.RequiredRole ?? string.Empty;
        
        await _unitOfWork.DashboardRawComponents.UpdateAsync(component.Id, component, cancellationToken);
    }
}
