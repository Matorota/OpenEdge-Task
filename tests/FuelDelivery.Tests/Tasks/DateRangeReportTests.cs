using FuelDelivery.Domain;
using FuelDelivery.Tasks.Bonus1;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Parsing;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tests.Tasks;

public class DateRangeReportTests
{
    private readonly DateRangeReportTask task = new(
        new ConsolePrompt(new StringReader(""), new StringWriter(), new DateParser()),
        new CsvWriter("output", "Test_User"));

    private readonly List<(Shipment Shipment, DeliveryPrice Price)> results;

    public DateRangeReportTests()
    {
        results =
        [
            Make("a", 2023, 12, 31, grandTotal: 500m, vat: 10.10m),
            Make("b", 2024, 1, 1, grandTotal: 900m, vat: 20.20m),
            Make("c", 2024, 1, 15, grandTotal: 900m, vat: 30.30m),
            Make("d", 2024, 1, 31, grandTotal: 100m, vat: 40.40m),
            Make("e", 2024, 2, 1, grandTotal: 999m, vat: 50.50m)
        ];
    }

    private (Shipment, DeliveryPrice) Make(string id, int year, int month, int day, decimal grandTotal, decimal vat) =>
        (new Shipment(id, FuelType.Diesel, ClientType.Private, new DateOnly(year, month, day), 1, 1, 1),
         new DeliveryPrice(0, 0, 0, vat, vat, grandTotal));

    [Fact]
    public void Includes_both_dates_and_takes_first_of_equal_totals() =>
        Assert.Equal(
            new DateRangeSummary(3, "b", 90.90m),
            task.Summarize(results, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

    [Fact]
    public void Empty_range_has_no_id() =>
        Assert.Equal(
            new DateRangeSummary(0, null, 0m),
            task.Summarize(results, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1)));
}
