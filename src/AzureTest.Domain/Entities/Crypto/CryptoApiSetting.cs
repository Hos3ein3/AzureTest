using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoApiSetting : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int MaskedKeyMaxLength = 100;
        public const int EncryptedKeyMaxLength = 500;
        public const int StatusMessageMaxLength = 500;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public CryptoExchange Exchange { get; private set; }
    public string ApiKeyMasked { get; private set; } = string.Empty;
    public string EncryptedApiKey { get; private set; } = string.Empty;
    public string EncryptedApiSecret { get; private set; } = string.Empty;
    public string? EncryptedPassphrase { get; private set; }
    public bool IsTestnet { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public bool IsConnected { get; private set; }
    public DateTime? LastTestedAtUtc { get; private set; }
    public string? ConnectionStatusMessage { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoApiSetting() { }

    private CryptoApiSetting(
        Guid userId,
        Guid tenantId,
        CryptoExchange exchange,
        string apiKeyMasked,
        string encryptedApiKey,
        string encryptedApiSecret,
        string? encryptedPassphrase,
        bool isTestnet)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Exchange = exchange;
        ApiKeyMasked = apiKeyMasked;
        EncryptedApiKey = encryptedApiKey;
        EncryptedApiSecret = encryptedApiSecret;
        EncryptedPassphrase = encryptedPassphrase;
        IsTestnet = isTestnet;
        IsActive = true;
        IsConnected = false;
    }

    public static CryptoApiSetting Create(
        Guid userId,
        Guid tenantId,
        CryptoExchange exchange,
        string rawApiKey,
        string encryptedApiKey,
        string encryptedApiSecret,
        string? encryptedPassphrase = null,
        bool isTestnet = true)
    {
        var masked = MaskApiKey(rawApiKey);
        return new CryptoApiSetting(
            userId,
            tenantId,
            exchange,
            masked,
            encryptedApiKey,
            encryptedApiSecret,
            encryptedPassphrase,
            isTestnet);
    }

    public static string MaskApiKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;
        var trimmed = key.Trim();
        if (trimmed.Length <= 8) return new string('*', trimmed.Length);
        return string.Concat(trimmed.AsSpan(0, 4), "...", trimmed.AsSpan(trimmed.Length - 4, 4));
    }

    public void UpdateKeys(
        string rawApiKey,
        string encryptedApiKey,
        string encryptedApiSecret,
        string? encryptedPassphrase,
        bool isTestnet)
    {
        ApiKeyMasked = MaskApiKey(rawApiKey);
        EncryptedApiKey = encryptedApiKey;
        EncryptedApiSecret = encryptedApiSecret;
        EncryptedPassphrase = encryptedPassphrase;
        IsTestnet = isTestnet;
        IsConnected = false;
        ConnectionStatusMessage = "Keys updated. Verification needed.";
    }

    public void MarkConnectionSuccess()
    {
        IsConnected = true;
        LastTestedAtUtc = DateTime.UtcNow;
        ConnectionStatusMessage = "Successfully connected (Read & Trade verified. Withdrawal blocked).";
    }

    public void MarkConnectionFailure(string error)
    {
        IsConnected = false;
        LastTestedAtUtc = DateTime.UtcNow;
        ConnectionStatusMessage = error;
    }
}
