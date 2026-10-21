using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoAlert : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMaxLength = 20;
        public const int MessageMaxLength = 500;
        public const int ChannelMaxLength = 50;
        public const int ValuePrecision = 18;
        public const int ValueScale = 8;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public CryptoAlertType AlertType { get; private set; }
    public decimal TargetValue { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string Channel { get; private set; } = "InApp"; // "InApp", "Telegram", "Email"
    public bool IsActive { get; private set; } = true;
    public bool IsTriggered { get; private set; }
    public DateTime? TriggeredAtUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoAlert() { }

    private CryptoAlert(
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoAlertType alertType,
        decimal targetValue,
        string message,
        string channel)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol;
        AlertType = alertType;
        TargetValue = targetValue;
        Message = message;
        Channel = channel;
        IsActive = true;
        IsTriggered = false;
    }

    public static CryptoAlert Create(
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoAlertType alertType,
        decimal targetValue,
        string? message = null,
        string? channel = null)
    {
        var safeMsg = string.IsNullOrWhiteSpace(message)
            ? $"{symbol} alert: {alertType} at {targetValue}"
            : message.Trim();

        return new CryptoAlert(
            userId,
            tenantId,
            symbol.Trim().ToUpperInvariant(),
            alertType,
            targetValue,
            safeMsg,
            string.IsNullOrWhiteSpace(channel) ? "InApp" : channel.Trim());
    }

    public void Trigger()
    {
        IsTriggered = true;
        TriggeredAtUtc = DateTime.UtcNow;
    }

    public void Reset()
    {
        IsTriggered = false;
        TriggeredAtUtc = null;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
