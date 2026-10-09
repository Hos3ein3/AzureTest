using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Events;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Queries;

public class GetAllEventsQueryHandler : BaseQueryHandler<GetAllEventsQuery, PagedResult<EventDto, GetAllEventsFilters>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEventsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllEventsQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<PagedResult<EventDto, GetAllEventsFilters>> ExecuteAsync(GetAllEventsQuery query, CancellationToken cancellationToken)
    {
        var filters = query.Filters ?? new GetAllEventsFilters();
        var targetUserId = query.UserId;
        var events = await _unitOfWork.Events.GetEventsForUserAsync(targetUserId);

        var dtos = events.Select(e => new EventDto(
            e.Id, e.Title, e.Description, e.StartDate, e.EndDate, e.IsAllDay, e.Color, e.UserId
        )).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var term = filters.SearchTerm.ToLower();
            dtos = dtos.Where(e => (e.Title != null && e.Title.ToLower().Contains(term))
                                   || (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        string sortBy = (filters.SortBy ?? "date").ToLower();
        bool desc = filters.SortDescending;

        dtos = sortBy switch
        {
            "title" => desc ? dtos.OrderByDescending(e => e.Title) : dtos.OrderBy(e => e.Title),
            "date" or "startdate" => desc ? dtos.OrderByDescending(e => e.StartDate) : dtos.OrderBy(e => e.StartDate),
            _ => desc ? dtos.OrderByDescending(e => e.StartDate) : dtos.OrderBy(e => e.StartDate)
        };

        var eventList = dtos.ToList();
        int totalCount = eventList.Count;
        int currentPage = filters.PageNumber < 1 ? 1 : filters.PageNumber;
        int pageSize = filters.PageSize < 1 ? 10 : filters.PageSize;

        var items = (filters.PageSize <= 0)
            ? eventList
            : eventList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<EventDto, GetAllEventsFilters>(items, totalCount, currentPage, pageSize, filters);
    }
}
