using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid UserId,
    Guid? NotificationId = null
) : ICommand<Result<int>>;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand, Result<int>>
{
    private readonly INoSqlUnitOfWork _noSqlUow;

    public MarkNotificationAsReadCommandHandler(INoSqlUnitOfWork noSqlUow)
    {
        _noSqlUow = noSqlUow ?? throw new ArgumentNullException(nameof(noSqlUow));
    }

    public async Task<Result<int>> HandleAsync(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken = default)
    {
        if (request.NotificationId.HasValue && request.NotificationId.Value != Guid.Empty)
        {
            var doc = await _noSqlUow.NotificationHistories.GetByIdAsync(request.NotificationId.Value, cancellationToken);
            if (doc != null && doc.UserId == request.UserId)
            {
                doc.MarkAsRead();
                await _noSqlUow.NotificationHistories.UpdateAsync(doc.Id, doc, cancellationToken);
                return Result<int>.Success(1);
            }

            return Result<int>.Success(0);
        }

        // Mark all unread notifications as read
        var unreadDocs = await _noSqlUow.NotificationHistories.GetByUserAsync(
            request.UserId,
            unreadOnly: true,
            limit: 200,
            cancellationToken);

        int updatedCount = 0;
        foreach (var doc in unreadDocs)
        {
            doc.MarkAsRead();
            await _noSqlUow.NotificationHistories.UpdateAsync(doc.Id, doc, cancellationToken);
            updatedCount++;
        }

        return Result<int>.Success(updatedCount);
    }
}
