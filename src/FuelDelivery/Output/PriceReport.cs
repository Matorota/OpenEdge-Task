using FuelDelivery.Pricing;

namespace FuelDelivery.Output;

/// <summary>Price file shared by Task 1 and Task 2.</summary>
public static class PriceReport
{
    public static void Write(string fileName, IEnumerable<Invoice> invoices) =>
        CsvFile.Write(fileName, "ShipmentID,NetFuelPrice,LogisticsCost,TotalTax,GrandTotal",
            invoices.Select(i => CsvFile.Row(
                i.Shipment.Id,
                Money.Format(i.NetFuelPrice),
                Money.Format(i.LogisticsCost),
                Money.Format(i.TotalTax),
                Money.Format(i.GrandTotal))));
}
