using System.Text.Json;
using FuelDelivery.Domain;

namespace FuelDelivery.Parsing;

public sealed record Rejection(string ShipmentId, ErrorCode Code);

public sealed record ShipmentFile(IReadOnlyList<Shipment> Shipments, IReadOnlyList<Rejection> Rejections);

/// <summary>Reads shipments.json and splits records into valid shipments and rejections.</summary>
public static class ShipmentReader
{
    private static readonly string[] RequiredFields = ["type", "client", "date", "volume", "distance", "basePrice"];

    private static readonly Dictionary<string, FuelType> FuelTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GASOLINE"] = FuelType.Gasoline,
        ["DIESEL"] = FuelType.Diesel,
        ["LPG"] = FuelType.Lpg
    };

    private static readonly Dictionary<string, ClientType> ClientTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PRIVATE"] = ClientType.Private,
        ["EXEMPT"] = ClientType.Exempt
    };

    public static ShipmentFile Parse(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("shipments.json must contain a JSON array.");

        var shipments = new List<Shipment>();
        var rejections = new List<Rejection>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (JsonElement record in document.RootElement.EnumerateArray())
        {
            Rejection? rejection = TryParseRecord(record, seenIds, out Shipment? shipment);
            if (rejection is null)
                shipments.Add(shipment!);
            else
                rejections.Add(rejection);
        }

        return new ShipmentFile(shipments, rejections);
    }

    /// <summary>Returns null when the record is valid, otherwise the first problem found.</summary>
    private static Rejection? TryParseRecord(JsonElement record, HashSet<string> seenIds, out Shipment? shipment)
    {
        shipment = null;
        if (record.ValueKind != JsonValueKind.Object || !TryGetField(record, "id", out JsonElement idElement))
            return new Rejection("", ErrorCode.MissingField);

        string id = idElement.ValueKind == JsonValueKind.String ? idElement.GetString()! : idElement.GetRawText();
        if (string.IsNullOrWhiteSpace(id))
            return new Rejection("", ErrorCode.MissingField);

        // The first occurrence claims the id, even if that record turns out to be invalid.
        if (!seenIds.Add(id))
            return new Rejection(id, ErrorCode.DuplicateId);
        if (idElement.ValueKind != JsonValueKind.String)
            return new Rejection(id, ErrorCode.InvalidValue);

        foreach (string field in RequiredFields)
        {
            if (!TryGetField(record, field, out _))
                return new Rejection(id, ErrorCode.MissingField);
        }

        if (!TryLookup(record.GetProperty("type"), FuelTypes, out FuelType type))
            return new Rejection(id, ErrorCode.UnknownType);
        if (!TryLookup(record.GetProperty("client"), ClientTypes, out ClientType client))
            return new Rejection(id, ErrorCode.UnknownClient);

        JsonElement dateElement = record.GetProperty("date");
        if (dateElement.ValueKind != JsonValueKind.String || !DateParser.TryParse(dateElement.GetString(), out DateOnly date))
            return new Rejection(id, ErrorCode.InvalidDate);

        if (!TryGetNumber(record, "volume", out decimal volume) || volume <= 0
            || !TryGetNumber(record, "distance", out decimal distance) || distance < 0
            || !TryGetNumber(record, "basePrice", out decimal basePrice) || basePrice < 0)
            return new Rejection(id, ErrorCode.InvalidValue);

        shipment = new Shipment(id, type, client, date, volume, distance, basePrice);
        return null;
    }

    private static bool TryGetField(JsonElement record, string name, out JsonElement value) =>
        record.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;

    private static bool TryLookup<T>(JsonElement element, Dictionary<string, T> values, out T result)
    {
        result = default!;
        return element.ValueKind == JsonValueKind.String
               && values.TryGetValue(element.GetString()!.Trim(), out result!);
    }

    // Numbers given as text ("6200") are invalid on purpose.
    private static bool TryGetNumber(JsonElement record, string name, out decimal value)
    {
        value = 0;
        JsonElement element = record.GetProperty(name);
        return element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out value);
    }
}
