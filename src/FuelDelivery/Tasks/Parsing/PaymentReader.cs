using System.Text.Json;

namespace FuelDelivery.Tasks.Parsing;

// Reads payments.json and keeps the first usable payment date for each known delivery id.
public class PaymentReader(DateParser dateParser)
{
    public Dictionary<string, DateOnly> Parse(string json, IReadOnlySet<string> knownIds)
    {
        var payments = new Dictionary<string, DateOnly>(StringComparer.Ordinal);

        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("payments.json must contain a JSON array.");

        foreach (JsonElement record in document.RootElement.EnumerateArray())
        {
            if (record.ValueKind != JsonValueKind.Object
                || !record.TryGetProperty("id", out JsonElement id) || id.ValueKind != JsonValueKind.String
                || !record.TryGetProperty("paidDate", out JsonElement paidDate) || paidDate.ValueKind != JsonValueKind.String)
                continue;

            string shipmentId = id.GetString()!;
            if (knownIds.Contains(shipmentId) && dateParser.TryParse(paidDate.GetString(), out DateOnly date))
                payments.TryAdd(shipmentId, date);
        }

        return payments;
    }
}
