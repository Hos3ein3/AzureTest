using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Tickets;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Queries;

public class GetTicketByIdQueryHandler : BaseQueryHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTicketByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTicketByIdQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<TicketDto?> ExecuteAsync(GetTicketByIdQuery query, CancellationToken cancellationToken)
    {
        var t = await _unitOfWork.Tickets.GetTicketWithMessagesAsync(query.Id, query.UserId);
        if (t == null) return null;
        
        return new TicketDto(
            t.Id, t.Subject, t.Content, t.Priority, t.Status, t.UserId, t.CreatedOn,
            t.Messages.OrderBy(m => m.CreatedOn).Select(m => new TicketMessageDto(
                m.Id, m.UserId, m.Content, m.IsReadByCreator, m.CreatedOn
            )).ToList()
        );
    }
}
