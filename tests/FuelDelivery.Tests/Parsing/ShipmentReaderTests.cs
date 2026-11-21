using FuelDelivery.Domain;
using FuelDelivery.Parsing;

namespace FuelDelivery.Tests.Parsing;

public class ShipmentReaderTests
{
    private static ErrorCode? CodeFor(string record)
    {
        ShipmentFile file = ShipmentReader.Parse($"[{record}]");
        return file.Rejections.Count == 0 ? null : file.Rejections[0].Code;
    }

    /// <summary>Builds a valid record with one field replaced (or removed with "&lt;missing&gt;").</summary>
    private static string RecordWith(string field, string jsonValue)
    {
        var fields = new Dictionary<string, string>
        {
            ["id"] = "\"a\"", ["type"] = "\"DIESEL\"", ["client"] = "\"PRIVATE\"", ["date"] = "\"2024-01-01\"",
            ["volume"] = "100", ["distance"] = "10", ["basePrice"] = "1"
        };
        if (jsonValue == "<missing>")
            fields.Remove(field);
        else
            fields[field] = jsonValue;
        return "{" + string.Join(",", fields.Select(f => $"\"{f.Key}\": {f.Value}")) + "}";
    }

    [Fact]
    public void Accepts_valid_record_with_case_and_spaces()
    {
        ShipmentFile file = ShipmentReader.Parse(
            "[{ \"id\": \"a\", \"type\": \"  gasoline \", \"client\": \" Exempt\", \"date\": \"15/01/2024\", \"volume\": 25000, \"distance\": 320, \"basePrice\": 0.89 }]");

        Shipment shipment = Assert.Single(file.Shipments);
        Assert.Equal(FuelType.Gasoline, shipment.Type);
        Assert.Equal(ClientType.Exempt, shipment.Client);
        Assert.Equal(new DateOnly(2024, 1, 15), shipment.Date);
        Assert.Equal(0.89m, shipment.BasePrice);
    }

    [Theory]
    [InlineData("type", "<missing>", ErrorCode.MissingField)]
    [InlineData("basePrice", "null", ErrorCode.MissingField)]
    [InlineData("type", "\"KEROSENE\"", ErrorCode.UnknownType)]
    [InlineData("type", "\"\"", ErrorCode.UnknownType)]
    [InlineData("type", "1", ErrorCode.UnknownType)]
    [InlineData("client", "\"GOVERNMENT\"", ErrorCode.UnknownClient)]
    [InlineData("date", "\"2023-02-30\"", ErrorCode.InvalidDate)]
    [InlineData("date", "20240101", ErrorCode.InvalidDate)]
    [InlineData("volume", "0", ErrorCode.InvalidValue)]
    [InlineData("volume", "-5", ErrorCode.InvalidValue)]
    [InlineData("volume", "\"100\"", ErrorCode.InvalidValue)]
    [InlineData("distance", "-1", ErrorCode.InvalidValue)]
    [InlineData("basePrice", "-0.01", ErrorCode.InvalidValue)]
    [InlineData("basePrice", "true", ErrorCode.InvalidValue)]
    [InlineData("distance", "0", null)]
    [InlineData("basePrice", "0", null)]
    public void Detects_problems(string field, string jsonValue, ErrorCode? expected) =>
        Assert.Equal(expected, CodeFor(RecordWith(field, jsonValue)));

    [Fact]
    public void Missing_id_leaves_shipment_id_empty()
    {
        ShipmentFile file = ShipmentReader.Parse($"[{RecordWith("id", "<missing>")}, {RecordWith("id", "null")}]");

        Assert.All(file.Rejections, r => Assert.Equal(new Rejection("", ErrorCode.MissingField), r));
    }

    [Fact]
    public void Duplicate_id_counts_even_if_first_record_was_invalid()
    {
        ShipmentFile file = ShipmentReader.Parse(
            $"[{RecordWith("type", "\"KEROSENE\"")}, {RecordWith("id", "\"a\"")}, {RecordWith("id", "\"A\"")}]");

        Assert.Equal([ErrorCode.UnknownType, ErrorCode.DuplicateId], file.Rejections.Select(r => r.Code));
        Assert.Equal("A", Assert.Single(file.Shipments).Id);
    }

    [Fact]
    public void Sample_file_gives_one_error_of_each_kind()
    {
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../../../../shipments.json"));
        ShipmentFile file = ShipmentReader.Parse(json);

        Assert.Equal(11, file.Shipments.Count);
        Assert.Equal(
            [ErrorCode.UnknownType, ErrorCode.InvalidValue, ErrorCode.InvalidDate, ErrorCode.MissingField, ErrorCode.UnknownClient, ErrorCode.DuplicateId],
            file.Rejections.Select(r => r.Code));
    }
}
