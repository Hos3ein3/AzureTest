using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Tickets;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Queries;

public class GetAllTicketsQueryHandler : BaseQueryHandler<GetAllTicketsQuery, PagedResult<TicketDto, GetAllTicketsFilters>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTicketsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllTicketsQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<PagedResult<TicketDto, GetAllTicketsFilters>> ExecuteAsync(GetAllTicketsQuery query, CancellationToken cancellationToken)
    {
        var filters = query.Filters ?? new GetAllTicketsFilters();
        var targetUserId = query.UserId;
        var tickets = await _unitOfWork.Tickets.GetAllTicketsAsync(targetUserId);

        var dtos = tickets.Select(t => new TicketDto(
            t.Id, t.Subject, t.Content, t.Priority, t.Status, t.UserId, t.CreatedOn,
            t.Messages.OrderBy(m => m.CreatedOn).Select(m => new TicketMessageDto(
                m.Id, m.UserId, m.Content, m.IsReadByCreator, m.CreatedOn
            )).ToList()
        )).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var term = filters.SearchTerm.ToLower();
            dtos = dtos.Where(t => (!string.IsNullOrEmpty(t.Subject) && t.Subject.ToLower().Contains(term)) ||
                                   (!string.IsNullOrEmpty(t.Content) && t.Content.ToLower().Contains(term)));
        }

        if (filters.Status.HasValue)
        {
            dtos = dtos.Where(t => t.Status == filters.Status.Value);
        }

        if (filters.Priority.HasValue)
        {
            dtos = dtos.Where(t => t.Priority == filters.Priority.Value);
        }

        bool isDesc = filters.SortDescending;
        string sortBy = (filters.SortBy ?? "date").ToLower();

        dtos = sortBy switch
        {
            "subject" => isDesc ? dtos.OrderByDescending(t => t.Subject) : dtos.OrderBy(t => t.Subject),
            "priority" => isDesc ? dtos.OrderByDescending(t => t.Priority) : dtos.OrderBy(t => t.Priority),
            "status" => isDesc ? dtos.OrderByDescending(t => t.Status) : dtos.OrderBy(t => t.Status),
            "date" => isDesc ? dtos.OrderByDescending(t => t.CreatedOn) : dtos.OrderBy(t => t.CreatedOn),
            _ => isDesc ? dtos.OrderByDescending(t => t.CreatedOn) : dtos.OrderBy(t => t.CreatedOn)
        };

        var ticketList = dtos.ToList();
        int totalCount = ticketList.Count;
        int currentPage = filters.PageNumber < 1 ? 1 : filters.PageNumber;
        int pageSize = filters.PageSize < 1 ? 10 : filters.PageSize;

        var items = (filters.PageSize <= 0)
            ? ticketList
            : ticketList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<TicketDto, GetAllTicketsFilters>(items, totalCount, currentPage, pageSize, filters);
    }
}
