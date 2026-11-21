using FuelDelivery.Tasks.Bonus1;
using FuelDelivery.Tasks.Bonus2;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Parsing;
using FuelDelivery.Tasks.Pricing;
using FuelDelivery.Tasks.Task1;
using FuelDelivery.Tasks.Task2;

// Create all objects once and pass them where they are needed.
var rules = new PricingRules();
var dateParser = new DateParser();
var calculator = new PriceCalculator(rules);
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
var task1Results = new StandardPricingTask(calculator, priceReport).Run(shipmentFile.Shipments);

// Bonus 2 asks nothing, so it runs before the questions.
new LateInterestTask(new LateInterestCalculator(rules), dateParser, csv).Run(task1Results, ReadPayments());

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

// A missing or broken payments file means no delivery has been paid.
Dictionary<string, DateOnly> ReadPayments()
{
    var knownIds = shipmentFile.Shipments.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);
    try
    {
        return new PaymentReader(dateParser).Parse(File.ReadAllText("payments.json"), knownIds);
    }
    catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
    {
        Console.Error.WriteLine($"Cannot read payments.json, treating all deliveries as unpaid: {e.Message}");
        return [];
    }
}
