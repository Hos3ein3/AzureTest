namespace AzureTest.Domain.Documents;

public class IdeaTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}