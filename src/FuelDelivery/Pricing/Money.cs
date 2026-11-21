using System.Globalization;

namespace FuelDelivery.Pricing;

public static class Money
{
    /// <summary>Rounds half up to 2 decimals (0.125 becomes 0.13).</summary>
    public static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>Always a dot, no thousands separator, exactly 2 decimals.</summary>
    public static string Format(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);
}
