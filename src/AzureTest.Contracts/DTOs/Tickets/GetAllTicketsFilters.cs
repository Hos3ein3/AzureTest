using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Tickets;

public class GetAllTicketsFilters : PagedFilterRequest
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? SortOrder
    {
        get => SortDescending ? "desc" : "asc";
        set => SortDescending = string.Equals(value, "desc", System.StringComparison.OrdinalIgnoreCase);
    }

    public GetAllTicketsFilters()
    {
        SortBy = "date";
        SortDescending = true;
    }
}
