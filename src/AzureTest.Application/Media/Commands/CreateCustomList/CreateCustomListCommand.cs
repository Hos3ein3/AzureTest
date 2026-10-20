using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.CreateCustomList;

public sealed record CreateCustomListCommand(
    Guid UserId,
    Guid TenantId,
    string Title,
    string? Description = null) : ICommand<Result<Guid>>;

public sealed class CreateCustomListCommandValidator : AbstractValidator<CreateCustomListCommand>
{
    public CreateCustomListCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("List title is required.")
            .MaximumLength(CustomMediaListDocument.Rules.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(CustomMediaListDocument.Rules.DescriptionMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public sealed class CreateCustomListCommandHandler : ICommandHandler<CreateCustomListCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;

    public CreateCustomListCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(CreateCustomListCommand cmd, CancellationToken ct = default)
    {
        var list = CustomMediaListDocument.Create(cmd.UserId, cmd.TenantId, cmd.Title, cmd.Description);
        await _uow.CustomMediaLists.AddAsync(list, ct);
        return Result.Success(list.Id);
    }
}
