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

public sealed record CreateUserDashboardCommand(
    Guid UserId,
    string Name,
    string Slug,
    List<DashboardSectionDto> Sections) : ICommand;

public class DashboardSectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<UserDashboardComponentDto> Components { get; set; } = new();
}

public class UserDashboardComponentDto
{
    public Guid ComponentId { get; set; }
    public Guid? RawComponentId { get; set; }
    public string Type { get; set; } = "Standard";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public sealed class CreateUserDashboardCommandHandler : ICommandHandler<CreateUserDashboardCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public CreateUserDashboardCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task HandleAsync(CreateUserDashboardCommand request, CancellationToken cancellationToken)
    {
        var dashboard = new UserDashboard
        {
            UserId = request.UserId,
            Name = request.Name,
            Slug = request.Slug,
            Sections = request.Sections.Select(s => new DashboardSection
            {
                Id = s.Id,
                Name = s.Name,
                Components = s.Components.Select(c => new UserDashboardComponent
                {
                    ComponentId = c.ComponentId,
                    RawComponentId = c.RawComponentId,
                    Type = c.Type,
                    X = c.X,
                    Y = c.Y,
                    Width = c.Width,
                    Height = c.Height
                }).ToList()
            }).ToList()
        };

        await _noSqlUnitOfWork.UserDashboards.AddAsync(dashboard, cancellationToken);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class CreateUserDashboardCommandValidator : AbstractValidator<CreateUserDashboardCommand>
{
    public CreateUserDashboardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
    }
}
