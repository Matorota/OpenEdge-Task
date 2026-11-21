using FuelDelivery.Domain;
using FuelDelivery.Output;
using FuelDelivery.Pricing;

namespace FuelDelivery.Tasks.Task1;

/// <summary>Task 1: prices every delivery with the standard rates.</summary>
public static class StandardPricingTask
{
    public static IReadOnlyList<Invoice> Run(IReadOnlyList<Shipment> shipments)
    {
        List<Invoice> invoices = shipments.Select(s => PriceCalculator.Calculate(s, TaxRates.Standard)).ToList();
        PriceReport.Write("task1.csv", invoices);
        return invoices;
    }
}
