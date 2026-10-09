namespace AzureTest.Domain.Primitives;

public interface IStatus
{
    public Status Stats { get; set; }
    DateTime? ChangedOn { get; set; }
    string? ChangedBy { get; set; }
}

public enum Status
{
    Accepted,
    Rejected,
    Completed,
    Canceled,
    Pending,
    
}