namespace AzureTest.Application.Companies.Queries.GetAllCompanies;

public sealed record GetAllCompaniesFilters(
    string? Name = null,
    bool? IsINDMember = null,
    string? Location = null,
    int PageNumber = 1,
    int PageSize = 10
);