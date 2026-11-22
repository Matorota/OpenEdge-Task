using FuelDelivery.Domain;

namespace FuelDelivery.Tasks.Pricing;

// All business rates, limits and rate-change dates.
public class PricingRules
{
    // Discounts on the net fuel price, for volumes above the limit.
    public decimal LargeVolumeLimit { get; } = 20000m;
    public decimal LargeVolumeDiscount { get; } = 0.10m;
    public decimal MediumVolumeLimit { get; } = 5000m;
    public decimal MediumVolumeDiscount { get; } = 0.05m;

    // Logistics: price per km, plus a fee for loads under the limit.
    public decimal DeliveryPricePerKm { get; } = 1.80m;
    public decimal SmallLoadLimit { get; } = 1000m;
    public decimal SmallLoadFee { get; } = 150.00m;

    public IReadOnlyDictionary<FuelType, decimal> ExcisePerLitre { get; } = new Dictionary<FuelType, decimal>
    {
        [FuelType.Gasoline] = 0.47m,
        [FuelType.Diesel] = 0.33m,
        [FuelType.Lpg] = 0.18m
    };

    // VAT: private rate changes on this date, exempt clients pay none.
    public DateOnly VatChangeDate { get; } = new(2024, 1, 1);
    public decimal PrivateVatBeforeChange { get; } = 0.21m;
    public decimal PrivateVatFromChange { get; } = 0.23m;
    public decimal ExemptVat { get; } = 0m;

    // Bonus 2: payment term, report date and yearly interest rates.
    public int PaymentTermDays { get; } = 30;
    public DateOnly ReportDate { get; } = new(2024, 7, 20);
    public DateOnly InterestChangeDate { get; } = new(2024, 7, 1);
    public decimal InterestBeforeChange { get; } = 0.12m;
    public decimal InterestFromChange { get; } = 0.09m;
    public int DaysPerYear { get; } = 365;
}
