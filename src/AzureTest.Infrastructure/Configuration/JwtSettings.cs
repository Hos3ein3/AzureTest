

namespace AzureTest.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int ExpirySeconds { get; set; }
    public int ExpirationSeconds { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
}
