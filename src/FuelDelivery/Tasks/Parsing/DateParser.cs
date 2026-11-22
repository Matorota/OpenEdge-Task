using System.Globalization;
using System.Text.RegularExpressions;

namespace FuelDelivery.Tasks.Parsing;

// Parses dates in YYYY-MM-DD or DD/MM/YYYY format. Anything else is invalid.
public class DateParser
{
    // The regular expressions check only the shape (digits and separators). TryParse then checks the date is real.
    private readonly Regex isoFormat = new(@"^(?<y>[0-9]{4})-(?<m>[0-9]{2})-(?<d>[0-9]{2})\z");
    private readonly Regex dayFirstFormat = new(@"^(?<d>[0-9]{2})/(?<m>[0-9]{2})/(?<y>[0-9]{4})\z");

    public bool TryParse(string? text, out DateOnly date)
    {
        date = default;
        if (text is null)
            return false;

        Match match = isoFormat.Match(text);
        if (!match.Success)
            match = dayFirstFormat.Match(text);
        if (!match.Success)
            return false;

        int year = int.Parse(match.Groups["y"].Value);
        int month = int.Parse(match.Groups["m"].Value);
        int day = int.Parse(match.Groups["d"].Value);

        // DaysInMonth knows leap years, so 2024-02-29 is valid and 2023-02-30 is not.
        if (year < 1 || month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            return false;

        date = new DateOnly(year, month, day);
        return true;
    }

    public string Format(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
