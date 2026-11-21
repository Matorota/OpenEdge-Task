using FuelDelivery.Domain;

namespace FuelDelivery.Tasks.Pricing;

/// <summary>Prices one delivery. Every step is rounded before it is used in the next one.</summary>
public class PriceCalculator(PricingRules rules)
{
    public DeliveryPrice Calculate(Shipment shipment, TaxOverride? taxOverride = null)
    {
        decimal netFuelPrice = RoundMoney(shipment.Volume * shipment.BasePrice * (1 - DiscountRate(shipment.Volume)));
        decimal logisticsCost = RoundMoney(shipment.Distance * rules.DeliveryPricePerKm) + SmallLoadFee(shipment.Volume);
        decimal excise = RoundMoney(shipment.Volume * ExcisePerLitre(shipment.Type, taxOverride));
        decimal vat = RoundMoney((netFuelPrice + logisticsCost + excise) * VatRate(shipment.Client, shipment.Date, taxOverride));
        decimal totalTax = excise + vat;
        decimal grandTotal = netFuelPrice + logisticsCost + totalTax;

        return new DeliveryPrice(netFuelPrice, logisticsCost, excise, vat, totalTax, grandTotal);
    }

    public decimal DiscountRate(decimal volume)
    {
        if (volume > rules.LargeVolumeLimit)
            return rules.LargeVolumeDiscount;
        if (volume > rules.MediumVolumeLimit)
            return rules.MediumVolumeDiscount;
        return 0m;
    }

    public decimal VatRate(ClientType client, DateOnly date, TaxOverride? taxOverride = null)
    {
        if (client == ClientType.Exempt)
            return rules.ExemptVat;
        if (taxOverride is not null)
            return taxOverride.PrivateVat;
        return date < rules.VatChangeDate ? rules.PrivateVatBeforeChange : rules.PrivateVatFromChange;
    }

    public decimal ExcisePerLitre(FuelType type, TaxOverride? taxOverride = null) =>
        type == FuelType.Gasoline && taxOverride is not null
            ? taxOverride.GasolineExcise
            : rules.ExcisePerLitre[type];

    private decimal SmallLoadFee(decimal volume) =>
        volume < rules.SmallLoadLimit ? rules.SmallLoadFee : 0m;

    // Half up: 0.125 becomes 0.13.
    private decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
