using FuelDelivery.Domain;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tasks.Bonus1;

public record DateRangeSummary(int ShipmentCount, string? MostExpensiveId, decimal TotalVat);

// Bonus 1: summary of Task 2 results between two dates (both included).
public class DateRangeReportTask(ConsolePrompt prompt, CsvWriter csv)
{
    public void Run(IReadOnlyList<(Shipment Shipment, DeliveryPrice Price)> task2Results)
    {
        DateOnly start = prompt.AskDate("Enter Start Date:");
        DateOnly end = prompt.AskDate("Enter End Date:", notBefore: start);

        DateRangeSummary summary = Summarize(task2Results, start, end);
        csv.Write("bonus1.csv", "ShipmentCount,MostExpensiveID,TotalVAT",
        [
            csv.Row(summary.ShipmentCount.ToString(), summary.MostExpensiveId ?? "N/A", csv.Money(summary.TotalVat))
        ]);
    }

    // Works without console or files, so it can be tested directly.
    public DateRangeSummary Summarize(
        IEnumerable<(Shipment Shipment, DeliveryPrice Price)> results, DateOnly start, DateOnly end)
    {
        var inRange = results.Where(r => r.Shipment.Date >= start && r.Shipment.Date <= end).ToList();

        // Strict ">" keeps the first one in the file when totals are equal.
        (Shipment Shipment, DeliveryPrice Price)? mostExpensive = null;
        foreach (var result in inRange)
        {
            if (mostExpensive is null || result.Price.GrandTotal > mostExpensive.Value.Price.GrandTotal)
                mostExpensive = result;
        }

        return new DateRangeSummary(inRange.Count, mostExpensive?.Shipment.Id, inRange.Sum(r => r.Price.Vat));
    }
}
