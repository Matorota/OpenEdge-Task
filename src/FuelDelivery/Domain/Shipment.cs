namespace FuelDelivery.Domain;

// A validated delivery, ready to be priced.
public record Shipment(
    string Id,
    FuelType Type,
    ClientType Client,
    DateOnly Date,
    decimal Volume,
    decimal Distance,
    decimal BasePrice);
