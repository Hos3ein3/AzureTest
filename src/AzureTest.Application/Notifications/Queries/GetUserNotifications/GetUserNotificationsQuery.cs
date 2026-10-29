using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.Domain.Documents.Notifications;
using FluentValidation;

namespace AzureTest.Application.Notifications.Queries.GetUserNotifications;

public sealed record GetUserNotificationsQuery(
    Guid UserId,
    bool? UnreadOnly = null,
    int Limit = 50
) : IQuery<Result<IReadOnlyList<NotificationDto>>>
{
    public static class Mapping
    {
        public static NotificationDto ToDto(NotificationHistoryDocument doc)
        {
            return new NotificationDto(
                doc.Id,
                doc.UserId,
                doc.Title,
                doc.Message,
                doc.Type,
                doc.ActionUrl,
                doc.IsRead,
                doc.ReadAt,
                doc.CreatedAt,
                CalculateTimeAgo(doc.CreatedAt),
                doc.Metadata
            );
        }

        private static string CalculateTimeAgo(DateTime utcDateTime)
        {
            var span = DateTime.UtcNow - utcDateTime;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return utcDateTime.ToString("MMM dd");
        }
    }
}

public sealed class GetUserNotificationsQueryValidator : AbstractValidator<GetUserNotificationsQuery>
{
    public GetUserNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Limit).InclusiveBetween(1, 200).WithMessage("Limit must be between 1 and 200.");
    }
}

public sealed class GetUserNotificationsQueryHandler : IQueryHandler<GetUserNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INoSqlUnitOfWork _noSqlUow;

    public GetUserNotificationsQueryHandler(INoSqlUnitOfWork noSqlUow)
    {
        _noSqlUow = noSqlUow ?? throw new ArgumentNullException(nameof(noSqlUow));
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> HandleAsync(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken = default)
    {
        var docs = await _noSqlUow.NotificationHistories.GetByUserAsync(
            request.UserId,
            request.UnreadOnly,
            request.Limit,
            cancellationToken);

        var dtos = docs.Select(GetUserNotificationsQuery.Mapping.ToDto).ToList();
        return Result<IReadOnlyList<NotificationDto>>.Success(dtos);
    }
}
