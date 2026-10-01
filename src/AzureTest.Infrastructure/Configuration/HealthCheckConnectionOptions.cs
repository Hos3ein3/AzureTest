

namespace AzureTest.Infrastructure.Configuration;

public class HealthCheckConnectionOptions
{
    public string SqlServer { get; set; } = string.Empty;
    public string CosmosDb { get; set; } = string.Empty;
    public string BlobStorage { get; set; } = string.Empty;
}
