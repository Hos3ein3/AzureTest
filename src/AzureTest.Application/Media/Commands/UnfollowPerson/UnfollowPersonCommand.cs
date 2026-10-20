using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.UnfollowPerson;

public sealed record UnfollowPersonCommand(
    Guid UserId,
    int TmdbPersonId) : ICommand<Result>;

public sealed class UnfollowPersonCommandValidator : AbstractValidator<UnfollowPersonCommand>
{
    public UnfollowPersonCommandValidator()
    {
        RuleFor(x => x.TmdbPersonId)
            .GreaterThan(0).WithMessage("Valid TMDb person ID is required.");
    }
}

public sealed class UnfollowPersonCommandHandler : ICommandHandler<UnfollowPersonCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public UnfollowPersonCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(UnfollowPersonCommand cmd, CancellationToken ct = default)
    {
        var existing = await _uow.FollowedPeople.GetByUserAndPersonAsync(cmd.UserId, cmd.TmdbPersonId, ct);
        if (existing == null)
        {
            return Result.Failure("Followed person not found.");
        }

        if (existing.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to unfollow this person.");
        }

        await _uow.FollowedPeople.RemoveAsync(existing.Id, ct);
        return Result.Success();
    }
}
