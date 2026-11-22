using System.Globalization;
using System.Text;

namespace FuelDelivery.Tasks.Output;

// Writes report files: author line, header, then one row per line.
public class CsvWriter(string outputFolder, string authorLine)
{
    // Uses "\n" line endings and UTF-8 without BOM, so files are the same on every computer.
    public void Write(string fileName, string header, IEnumerable<string> rows)
    {
        var content = new StringBuilder();
        content.Append(authorLine).Append('\n');
        content.Append(header).Append('\n');
        foreach (string row in rows)
            content.Append(row).Append('\n');

        Directory.CreateDirectory(outputFolder);
        File.WriteAllText(Path.Combine(outputFolder, fileName), content.ToString(), new UTF8Encoding(false));
    }

    public string Row(params string[] values) => string.Join(',', values.Select(Escape));

    // Always a dot, no thousands separator, exactly 2 decimals.
    public string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    // Quotes a value only when it contains a comma, quote or line break.
    private string Escape(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r']) < 0 ? value : $"\"{value.Replace("\"", "\"\"")}\"";
}
