namespace FuelDelivery.Domain;

/// <summary>A validated delivery, ready to be priced.</summary>
public sealed record Shipment(
    string Id,
    FuelType Type,
    ClientType Client,
    DateOnly Date,
    decimal Volume,
    decimal Distance,
    decimal BasePrice);
