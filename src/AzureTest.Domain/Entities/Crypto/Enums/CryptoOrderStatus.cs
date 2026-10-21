namespace AzureTest.Domain.Entities.Crypto.Enums;

public enum CryptoOrderStatus
{
    Pending = 1,
    Filled = 2,
    PartiallyFilled = 3,
    Cancelled = 4,
    Rejected = 5
}
