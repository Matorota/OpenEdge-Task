using FuelDelivery.Output;
using FuelDelivery.Parsing;
using FuelDelivery.Tasks.Task1;

ShipmentFile shipmentFile;
try
{
    shipmentFile = ShipmentReader.Parse(File.ReadAllText("shipments.json"));
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
{
    Console.Error.WriteLine($"Cannot read shipments.json: {e.Message}");
    return 1;
}

ErrorReport.Write(shipmentFile.Rejections);
StandardPricingTask.Run(shipmentFile.Shipments);

return 0;
