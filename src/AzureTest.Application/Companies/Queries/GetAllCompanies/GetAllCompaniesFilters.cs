
namespace AzureTest.Application.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesFilters
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public bool? IsINDMember { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
