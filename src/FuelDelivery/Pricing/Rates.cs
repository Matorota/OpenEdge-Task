using FuelDelivery.Domain;

namespace FuelDelivery.Pricing;

/// <summary>All business rates, limits and rate-change dates.</summary>
public static class Rates
{
    public const decimal LargeVolumeLimit = 20000m;
    public const decimal LargeVolumeDiscount = 0.10m;
    public const decimal MediumVolumeLimit = 5000m;
    public const decimal MediumVolumeDiscount = 0.05m;

    public const decimal DeliveryPricePerKm = 1.80m;
    public const decimal SmallLoadLimit = 1000m;
    public const decimal SmallLoadFee = 150.00m;

    public static readonly IReadOnlyDictionary<FuelType, decimal> ExcisePerLitre = new Dictionary<FuelType, decimal>
    {
        [FuelType.Gasoline] = 0.47m,
        [FuelType.Diesel] = 0.33m,
        [FuelType.Lpg] = 0.18m
    };

    public static readonly DateOnly VatChangeDate = new(2024, 1, 1);
    public const decimal PrivateVatBeforeChange = 0.21m;
    public const decimal PrivateVatFromChange = 0.23m;
    public const decimal ExemptVat = 0m;
}
