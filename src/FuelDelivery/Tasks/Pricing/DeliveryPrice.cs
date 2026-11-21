namespace FuelDelivery.Tasks.Pricing;

/// <summary>The calculated price of one delivery.</summary>
public record DeliveryPrice(
    decimal NetFuelPrice,
    decimal LogisticsCost,
    decimal Excise,
    decimal Vat,
    decimal TotalTax,
    decimal GrandTotal);
