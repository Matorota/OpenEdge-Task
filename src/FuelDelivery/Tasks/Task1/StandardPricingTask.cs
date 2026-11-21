using FuelDelivery.Domain;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tasks.Task1;

/// <summary>Task 1: prices every delivery with the standard rates.</summary>
public class StandardPricingTask(PriceCalculator calculator, PriceReport report)
{
    public List<(Shipment Shipment, DeliveryPrice Price)> Run(IReadOnlyList<Shipment> shipments)
    {
        var pricedShipments = shipments.Select(s => (s, calculator.Calculate(s))).ToList();
        report.Write("task1.csv", pricedShipments);
        return pricedShipments;
    }
}
