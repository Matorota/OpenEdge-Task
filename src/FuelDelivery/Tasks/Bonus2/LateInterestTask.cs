using FuelDelivery.Domain;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Parsing;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tasks.Bonus2;

// Bonus 2: late payment interest for every Task 1 result.
public class LateInterestTask(LateInterestCalculator calculator, DateParser dateParser, CsvWriter csv)
{
    public void Run(
        IReadOnlyList<(Shipment Shipment, DeliveryPrice Price)> task1Results,
        IReadOnlyDictionary<string, DateOnly> payments)
    {
        csv.Write("bonus2.csv", "ShipmentID,DueDate,DaysLate,LateInterest",
            task1Results.Select(result =>
            {
                DateOnly? paidDate = payments.TryGetValue(result.Shipment.Id, out DateOnly date) ? date : null;
                LatePayment late = calculator.Calculate(result.Shipment.Date, result.Price.GrandTotal, paidDate);

                return csv.Row(
                    result.Shipment.Id,
                    dateParser.Format(late.DueDate),
                    late.DaysLate.ToString(),
                    csv.Money(late.Interest));
            }));
    }
}
