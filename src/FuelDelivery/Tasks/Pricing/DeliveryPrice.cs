namespace FuelDelivery.Tasks.Pricing;

// The calculated price of one delivery.
public record DeliveryPrice(
    decimal NetFuelPrice,
    decimal LogisticsCost,
    decimal Excise,
    decimal Vat,
    decimal TotalTax,
    decimal GrandTotal);
