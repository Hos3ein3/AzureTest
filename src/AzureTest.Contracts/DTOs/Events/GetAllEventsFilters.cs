using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Events;

public class GetAllEventsFilters : PagedFilterRequest
{
    public GetAllEventsFilters()
    {
        SortBy = "StartDate";
        SortDescending = false;
    }
}
