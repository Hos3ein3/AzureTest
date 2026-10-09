namespace AzureTest.Domain.Primitives;

public interface IConcurrencyStamp
{
    public Guid ConcurrencyStamp { get; set; }
    
}