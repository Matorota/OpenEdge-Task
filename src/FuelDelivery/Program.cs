using FuelDelivery.Tasks.Bonus1;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Parsing;
using FuelDelivery.Tasks.Pricing;
using FuelDelivery.Tasks.Task1;
using FuelDelivery.Tasks.Task2;

// Create all objects once and pass them where they are needed.
var dateParser = new DateParser();
var calculator = new PriceCalculator(new PricingRules());
var csv = new CsvWriter("output", "Matas_Strimaitis");
var priceReport = new PriceReport(csv);
var prompt = new ConsolePrompt(Console.In, Console.Out, dateParser, echoAnswers: Console.IsInputRedirected);

ShipmentFile shipmentFile;
try
{
    shipmentFile = new ShipmentReader(dateParser).Parse(File.ReadAllText("shipments.json"));
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
{
    Console.Error.WriteLine($"Cannot read shipments.json: {e.Message}");
    return 1;
}

new ErrorReport(csv).Write(shipmentFile.Rejections);
new StandardPricingTask(calculator, priceReport).Run(shipmentFile.Shipments);

try
{
    var task2Results = new WhatIfPricingTask(calculator, priceReport, prompt).Run(shipmentFile.Shipments);
    new DateRangeReportTask(prompt, csv).Run(task2Results);
}
catch (EndOfStreamException e)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

return 0;
