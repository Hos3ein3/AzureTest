using System;
using System.Globalization;

namespace AzureTest.Application.Trading.Services;

public static class TradingNotificationFormatter
{
    public static string FormatPrice(decimal price)
    {
        if (price <= 0m) return "$0.00";
        if (price < 0.0001m)
            return "$" + price.ToString("0.00000000", CultureInfo.InvariantCulture);
        if (price < 0.01m)
            return "$" + price.ToString("0.000000", CultureInfo.InvariantCulture);
        if (price < 1m)
            return "$" + price.ToString("0.00000", CultureInfo.InvariantCulture);
        if (price < 1000m)
            return "$" + price.ToString("0.0000#", CultureInfo.InvariantCulture);
        return "$" + price.ToString("#,##0.00", CultureInfo.InvariantCulture);
    }

    public static string FormatPnl(decimal pnl)
    {
        if (pnl == 0m)
            return "$0.00000";

        var sign = pnl > 0m ? "+" : "-";
        var abs = Math.Abs(pnl);

        if (abs < 0.0001m)
            return $"{sign}${abs.ToString("0.00000000", CultureInfo.InvariantCulture)}";
        if (abs < 1000m)
            return $"{sign}${abs.ToString("0.00000", CultureInfo.InvariantCulture)}";

        return $"{sign}${abs.ToString("#,##0.00", CultureInfo.InvariantCulture)}";
    }

    public static string FormatQuantity(decimal qty)
    {
        if (qty == 0m) return "0";
        if (qty < 0.0001m) return qty.ToString("0.00000000", CultureInfo.InvariantCulture);
        if (qty < 0.01m) return qty.ToString("0.000000", CultureInfo.InvariantCulture);
        if (qty < 1m) return qty.ToString("0.00000", CultureInfo.InvariantCulture);
        if (qty < 1000m) return qty.ToString("0.0000", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
        return qty.ToString("#,##0.##", CultureInfo.InvariantCulture);
    }
}
