using FuelDelivery.Domain;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tasks.Output;

/// <summary>Price file shared by Task 1 and Task 2.</summary>
public class PriceReport(CsvWriter csv)
{
    public void Write(string fileName, IEnumerable<(Shipment Shipment, DeliveryPrice Price)> pricedShipments) =>
        csv.Write(fileName, "ShipmentID,NetFuelPrice,LogisticsCost,TotalTax,GrandTotal",
            pricedShipments.Select(p => csv.Row(
                p.Shipment.Id,
                csv.Money(p.Price.NetFuelPrice),
                csv.Money(p.Price.LogisticsCost),
                csv.Money(p.Price.TotalTax),
                csv.Money(p.Price.GrandTotal))));
}
