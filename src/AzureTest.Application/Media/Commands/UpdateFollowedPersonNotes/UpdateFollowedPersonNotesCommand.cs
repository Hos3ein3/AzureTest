using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.UpdateFollowedPersonNotes;

public sealed record UpdateFollowedPersonNotesCommand(
    Guid UserId,
    int TmdbPersonId,
    string? Notes) : ICommand<Result>;

public sealed class UpdateFollowedPersonNotesCommandValidator : AbstractValidator<UpdateFollowedPersonNotesCommand>
{
    public UpdateFollowedPersonNotesCommandValidator()
    {
        RuleFor(x => x.TmdbPersonId)
            .GreaterThan(0).WithMessage("Valid TMDb person ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(FollowedPersonDocument.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public sealed class UpdateFollowedPersonNotesCommandHandler : ICommandHandler<UpdateFollowedPersonNotesCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public UpdateFollowedPersonNotesCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(UpdateFollowedPersonNotesCommand cmd, CancellationToken ct = default)
    {
        var person = await _uow.FollowedPeople.GetByUserAndPersonAsync(cmd.UserId, cmd.TmdbPersonId, ct);
        if (person == null)
        {
            return Result.Failure("Followed person not found.");
        }

        if (person.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to update this person.");
        }

        person.UpdateNotes(cmd.Notes);
        await _uow.FollowedPeople.UpdateAsync(person.Id, person, ct);
        return Result.Success();
    }
}
