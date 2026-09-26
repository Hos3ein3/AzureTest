using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Tickets;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Queries;

public class GetAllTicketsQueryHandler : BaseQueryHandler<GetAllTicketsQuery, IReadOnlyList<TicketDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTicketsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllTicketsQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<IReadOnlyList<TicketDto>> ExecuteAsync(GetAllTicketsQuery query, CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.Tickets.GetAllTicketsAsync(query.UserId);

        return tickets.Select(t => new TicketDto(
            t.Id, t.Subject, t.Content, t.Priority, t.Status, t.UserId, t.CreatedOn,
            t.Messages.OrderBy(m => m.CreatedOn).Select(m => new TicketMessageDto(
                m.Id, m.UserId, m.Content, m.IsReadByCreator, m.CreatedOn
            )).ToList()
        )).ToList();
    }
}
