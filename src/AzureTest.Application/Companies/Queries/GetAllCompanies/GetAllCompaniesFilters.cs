using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesFilters : PagedFilterRequest
{
    public string? Name
    {
        get => SearchTerm;
        set => SearchTerm = value;
    }
    public string? Location { get; set; }
    public bool? IsINDMember { get; set; }
}
