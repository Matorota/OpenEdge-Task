using System.Text;

namespace FuelDelivery.Output;

/// <summary>Writes report files: author line, header, then one row per line.</summary>
public static class CsvFile
{
    public const string AuthorLine = "Matas_Strimaitis";
    public const string OutputFolder = "output";

    public static void Write(string fileName, string header, IEnumerable<string> rows)
    {
        var content = new StringBuilder();
        content.Append(AuthorLine).Append('\n');
        content.Append(header).Append('\n');
        foreach (string row in rows)
            content.Append(row).Append('\n');

        Directory.CreateDirectory(OutputFolder);
        File.WriteAllText(Path.Combine(OutputFolder, fileName), content.ToString(), new UTF8Encoding(false));
    }

    public static string Row(params string[] values) => string.Join(',', values.Select(Escape));

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r']) < 0 ? value : $"\"{value.Replace("\"", "\"\"")}\"";
}
