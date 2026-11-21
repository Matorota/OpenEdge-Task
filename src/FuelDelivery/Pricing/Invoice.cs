using FuelDelivery.Domain;

namespace FuelDelivery.Pricing;

public sealed record Invoice(
    Shipment Shipment,
    decimal NetFuelPrice,
    decimal LogisticsCost,
    decimal Excise,
    decimal Vat,
    decimal TotalTax,
    decimal GrandTotal);
