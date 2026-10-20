using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Contracts.DTOs.Media;

public class WatchListFilter : PagedFilterRequest
{
    public WatchStatus? Status { get; set; }
    public MediaType? Type { get; set; }
    public string? Genre { get; set; }
}
