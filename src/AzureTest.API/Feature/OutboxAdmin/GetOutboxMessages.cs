using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class GetOutboxMessages
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetOutboxMessagesFilterDto filter,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 5, 100);

        var (items, totalCount) = await uow.OutboxMessages.GetPagedAsync(
            filter.SearchTerm,
            filter.IsDone,
            filter.FromDate,
            filter.ToDate,
            filter.SortBy,
            filter.SortDescending,
            pageNumber,
            pageSize,
            cancellationToken);

        var (_, pendingCount) = await uow.OutboxMessages.GetPagedAsync(
            isDone: false,
            pageNumber: 1,
            pageSize: 1,
            ct: cancellationToken);

        var (_, processedCount) = await uow.OutboxMessages.GetPagedAsync(
            isDone: true,
            pageNumber: 1,
            pageSize: 1,
            ct: cancellationToken);

        var messagesDto = items.Select(m => new OutboxMessageDto(
            m.Id,
            m.EventType,
            m.Payload,
            m.OccurredOnUtc,
            m.ProcessedOnUtc,
            m.Error,
            m.RetryCount
        )).ToList();

        var response = new OutboxMessageListResponseDto
        {
            Messages = messagesDto,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = filter.SearchTerm,
            IsDone = filter.IsDone,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending,
            PendingCount = pendingCount,
            ProcessedCount = processedCount,
            FailedCount = items.Count(m => m.RetryCount > 0 || !string.IsNullOrWhiteSpace(m.Error))
        };

        return Results.Ok(response);
    }
}
