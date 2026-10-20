using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.FollowPerson;

public sealed record FollowPersonCommand(
    Guid UserId,
    Guid TenantId,
    int TmdbPersonId,
    string Name,
    string? Department = "Acting",
    string? ProfilePath = null,
    string? Notes = null) : ICommand<Result<Guid>>;

public sealed class FollowPersonCommandValidator : AbstractValidator<FollowPersonCommand>
{
    public FollowPersonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Person name is required.")
            .MaximumLength(FollowedPersonDocument.Rules.NameMaxLength);

        RuleFor(x => x.TmdbPersonId)
            .GreaterThan(0).WithMessage("Valid TMDb person ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(FollowedPersonDocument.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public sealed class FollowPersonCommandHandler : ICommandHandler<FollowPersonCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;

    public FollowPersonCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(FollowPersonCommand cmd, CancellationToken ct = default)
    {
        var existing = await _uow.FollowedPeople.GetByUserAndPersonAsync(cmd.UserId, cmd.TmdbPersonId, ct);
        if (existing != null)
        {
            return Result.Success(existing.Id);
        }

        var person = FollowedPersonDocument.Create(
            cmd.UserId,
            cmd.TenantId,
            cmd.TmdbPersonId,
            cmd.Name,
            cmd.Department,
            cmd.ProfilePath,
            cmd.Notes);

        await _uow.FollowedPeople.AddAsync(person, ct);
        return Result.Success(person.Id);
    }
}
