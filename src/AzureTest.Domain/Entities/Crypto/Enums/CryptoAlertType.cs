namespace AzureTest.Domain.Entities.Crypto.Enums;

public enum CryptoAlertType
{
    PriceAbove = 1,
    PriceBelow = 2,
    RsiOverbought = 3,
    RsiOversold = 4,
    MacdCross = 5,
    WhaleAlert = 6
}
