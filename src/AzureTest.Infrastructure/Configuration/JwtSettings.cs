namespace AzureTest.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpirySeconds { get; set; }
    public int ExpirationSeconds { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
}
