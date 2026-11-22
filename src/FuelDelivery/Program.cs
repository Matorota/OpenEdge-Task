using FuelDelivery.Tasks.Bonus1;
using FuelDelivery.Tasks.Bonus2;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Parsing;
using FuelDelivery.Tasks.Pricing;
using FuelDelivery.Tasks.Task1;
using FuelDelivery.Tasks.Task2;

// 1. Choose what to run. No argument runs everything (required by the task). An argument runs one part, for example "task2".
string part = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "all";
if (part is not ("all" or "task1" or "task2" or "bonus1" or "bonus2"))
{
    Console.Error.WriteLine($"Unknown part '{args[0]}'. Use task1, task2, bonus1, bonus2 or no argument for all.");
    return 1;
}

// Bonus 2 needs Task 1 results, Bonus 1 needs Task 2 results.
bool runTask1 = part is "all" or "task1" or "bonus2";
bool runBonus2 = part is "all" or "bonus2";
bool runTask2 = part is "all" or "task2" or "bonus1";
bool runBonus1 = part is "all" or "bonus1";

// 2. Create all objects once and pass them where they are needed.
var rules = new PricingRules();
var dateParser = new DateParser();
var calculator = new PriceCalculator(rules);
var csv = new CsvWriter("output", "Matas_Strimaitis");
var priceReport = new PriceReport(csv);
var prompt = new ConsolePrompt(Console.In, Console.Out, dateParser, echoAnswers: Console.IsInputRedirected);

// 3. Read and validate shipments.json. Without it nothing can be calculated, so the program stops.
ShipmentFile shipmentFile;
try
{
    shipmentFile = new ShipmentReader(dateParser).Parse(File.ReadAllText("shipments.json"));
}
catch (Exception e) when (IsFileProblem(e))
{
    Console.Error.WriteLine($"Cannot read shipments.json: {e.Message}");
    return 1;
}

new ErrorReport(csv).Write(shipmentFile.Rejections);

// 4. Run the parts. Questions are asked only by Task 2 and Bonus 1.
try
{
    if (runTask1)
    {
        var task1Results = new StandardPricingTask(calculator, priceReport).Run(shipmentFile.Shipments);

        // Bonus 2 asks nothing, so it runs before the questions.
        if (runBonus2)
            new LateInterestTask(new LateInterestCalculator(rules), dateParser, csv).Run(task1Results, ReadPayments());
    }

    if (runTask2)
    {
        var task2Results = new WhatIfPricingTask(calculator, priceReport, prompt).Run(shipmentFile.Shipments);

        if (runBonus1)
            new DateRangeReportTask(prompt, csv).Run(task2Results);
    }
}
catch (Exception e) when (e is EndOfStreamException or OverflowException)
{
    // Input ended too early, or a custom rate is so large that the price cannot be calculated.
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
    catch (Exception e) when (IsFileProblem(e))
    {
        Console.Error.WriteLine($"Cannot read payments.json, treating all deliveries as unpaid: {e.Message}");
        return [];
    }
}

// Missing file, no access, broken JSON, or JSON that is not a list.
bool IsFileProblem(Exception e) =>
    e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException;
