namespace AzureTest.Domain.Documents;

public class Company
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string CompanyName { get; set; } = string.Empty;
    public string? KvkNumber { get; set; } = null;
    public bool IsINDMember { get; set; } = false;
    public string? LinkedinURL { get; set; } = null;
    public string? IndeedUrl { get; set; } = null;

    public string? Location { get; set; } = null;


    
}