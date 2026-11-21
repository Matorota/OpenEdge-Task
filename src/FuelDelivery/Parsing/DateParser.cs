using System.Globalization;
using System.Text.RegularExpressions;

namespace FuelDelivery.Parsing;

/// <summary>Parses dates in YYYY-MM-DD or DD/MM/YYYY format. Anything else is invalid.</summary>
public static partial class DateParser
{
    [GeneratedRegex(@"^(?<y>[0-9]{4})-(?<m>[0-9]{2})-(?<d>[0-9]{2})\z")]
    private static partial Regex IsoFormat();

    [GeneratedRegex(@"^(?<d>[0-9]{2})/(?<m>[0-9]{2})/(?<y>[0-9]{4})\z")]
    private static partial Regex DayFirstFormat();

    public static bool TryParse(string? text, out DateOnly date)
    {
        date = default;
        if (text is null)
            return false;

        Match match = IsoFormat().Match(text);
        if (!match.Success)
            match = DayFirstFormat().Match(text);
        if (!match.Success)
            return false;

        int year = int.Parse(match.Groups["y"].Value);
        int month = int.Parse(match.Groups["m"].Value);
        int day = int.Parse(match.Groups["d"].Value);

        if (year < 1 || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            return false;

        date = new DateOnly(year, month, day);
        return true;
    }

    public static string Format(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
