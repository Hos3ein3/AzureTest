using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.RateWatchItem;

public sealed record RateWatchItemCommand(
    Guid UserId,
    Guid WatchItemId,
    int? Rating,
    string? Review,
    DateTime? WatchedDate = null) : ICommand<Result>;

public sealed class RateWatchItemCommandValidator : AbstractValidator<RateWatchItemCommand>
{
    public RateWatchItemCommandValidator()
    {
        RuleFor(x => x.WatchItemId)
            .NotEmpty().WithMessage("Watch item ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(WatchItemDocument.Rules.MinRating, WatchItemDocument.Rules.MaxRating)
            .When(x => x.Rating.HasValue)
            .WithMessage($"Rating must be between {WatchItemDocument.Rules.MinRating} and {WatchItemDocument.Rules.MaxRating}.");

        RuleFor(x => x.Review)
            .MaximumLength(WatchItemDocument.Rules.ReviewMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Review))
            .WithMessage($"Review cannot exceed {WatchItemDocument.Rules.ReviewMaxLength} characters.");
    }
}

public sealed class RateWatchItemCommandHandler : ICommandHandler<RateWatchItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;

    public RateWatchItemCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(RateWatchItemCommand cmd, CancellationToken ct = default)
    {
        var item = await _uow.WatchItems.GetByIdAsync(cmd.WatchItemId, ct);
        if (item == null)
        {
            return Result.Failure("Watch item not found.");
        }

        if (item.UserId != cmd.UserId)
        {
            return Result.Failure("Not authorized to rate this watch item.");
        }

        item.RateAndReview(cmd.Rating, cmd.Review, cmd.WatchedDate);
        await _uow.WatchItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}
