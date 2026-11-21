using System.Text.Json;
using FuelDelivery.Domain;

namespace FuelDelivery.Tasks.Parsing;

public enum ErrorCode
{
    MissingField,
    UnknownType,
    UnknownClient,
    InvalidDate,
    InvalidValue,
    DuplicateId
}

public record Rejection(string ShipmentId, ErrorCode Code);

public record ShipmentFile(IReadOnlyList<Shipment> Shipments, IReadOnlyList<Rejection> Rejections);

/// <summary>Reads shipments.json and splits records into valid shipments and rejections.</summary>
public class ShipmentReader(DateParser dateParser)
{
    private readonly string[] requiredFields = ["type", "client", "date", "volume", "distance", "basePrice"];

    private readonly Dictionary<string, FuelType> fuelTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GASOLINE"] = FuelType.Gasoline,
        ["DIESEL"] = FuelType.Diesel,
        ["LPG"] = FuelType.Lpg
    };

    private readonly Dictionary<string, ClientType> clientTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PRIVATE"] = ClientType.Private,
        ["EXEMPT"] = ClientType.Exempt
    };

    public ShipmentFile Parse(string json)
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
    private Rejection? TryParseRecord(JsonElement record, HashSet<string> seenIds, out Shipment? shipment)
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

        foreach (string field in requiredFields)
        {
            if (!TryGetField(record, field, out _))
                return new Rejection(id, ErrorCode.MissingField);
        }

        if (!TryLookup(record.GetProperty("type"), fuelTypes, out FuelType type))
            return new Rejection(id, ErrorCode.UnknownType);
        if (!TryLookup(record.GetProperty("client"), clientTypes, out ClientType client))
            return new Rejection(id, ErrorCode.UnknownClient);

        JsonElement dateElement = record.GetProperty("date");
        if (dateElement.ValueKind != JsonValueKind.String || !dateParser.TryParse(dateElement.GetString(), out DateOnly date))
            return new Rejection(id, ErrorCode.InvalidDate);

        if (!TryGetNumber(record, "volume", out decimal volume) || volume <= 0
            || !TryGetNumber(record, "distance", out decimal distance) || distance < 0
            || !TryGetNumber(record, "basePrice", out decimal basePrice) || basePrice < 0)
            return new Rejection(id, ErrorCode.InvalidValue);

        shipment = new Shipment(id, type, client, date, volume, distance, basePrice);
        return null;
    }

    private bool TryGetField(JsonElement record, string name, out JsonElement value) =>
        record.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;

    private bool TryLookup<T>(JsonElement element, Dictionary<string, T> values, out T result)
    {
        result = default!;
        return element.ValueKind == JsonValueKind.String
               && values.TryGetValue(element.GetString()!.Trim(), out result!);
    }

    // Numbers given as text ("6200") are invalid on purpose.
    private bool TryGetNumber(JsonElement record, string name, out decimal value)
    {
        value = 0;
        JsonElement element = record.GetProperty(name);
        return element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out value);
    }
}
