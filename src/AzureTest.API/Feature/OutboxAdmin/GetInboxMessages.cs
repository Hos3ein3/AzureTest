using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class GetInboxMessages
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetInboxMessagesFilterDto filter,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 5, 100);

        var (items, totalCount) = await uow.InboxMessages.GetPagedAsync(
            filter.SearchTerm,
            filter.FromDate,
            filter.ToDate,
            filter.SortBy,
            filter.SortDescending,
            pageNumber,
            pageSize,
            cancellationToken);

        var messagesDto = items.Select(m => new InboxMessageDto(
            m.Id,
            m.HandlerName,
            m.ReceivedOnUtc,
            m.ProcessedOnUtc
        )).ToList();

        var response = new InboxMessageListResponseDto
        {
            Messages = messagesDto,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = filter.SearchTerm,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending,
            TotalProcessedCount = totalCount
        };

        return Results.Ok(response);
    }
}
