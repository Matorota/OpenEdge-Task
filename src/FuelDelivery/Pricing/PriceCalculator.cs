using FuelDelivery.Domain;

namespace FuelDelivery.Pricing;

/// <summary>Prices one delivery. Every step is rounded before it is used in the next one.</summary>
public static class PriceCalculator
{
    public static Invoice Calculate(Shipment shipment, TaxRates rates)
    {
        decimal netFuelPrice = Money.Round(shipment.Volume * shipment.BasePrice * (1 - DiscountRate(shipment.Volume)));
        decimal logisticsCost = Money.Round(shipment.Distance * Rates.DeliveryPricePerKm) + SmallLoadFee(shipment.Volume);
        decimal excise = Money.Round(shipment.Volume * rates.ExcisePerLitre(shipment.Type));
        decimal vat = Money.Round((netFuelPrice + logisticsCost + excise) * rates.VatRate(shipment.Client, shipment.Date));
        decimal totalTax = excise + vat;
        decimal grandTotal = netFuelPrice + logisticsCost + totalTax;

        return new Invoice(shipment, netFuelPrice, logisticsCost, excise, vat, totalTax, grandTotal);
    }

    public static decimal DiscountRate(decimal volume) => volume switch
    {
        > Rates.LargeVolumeLimit => Rates.LargeVolumeDiscount,
        > Rates.MediumVolumeLimit => Rates.MediumVolumeDiscount,
        _ => 0m
    };

    private static decimal SmallLoadFee(decimal volume) =>
        volume < Rates.SmallLoadLimit ? Rates.SmallLoadFee : 0m;
}
