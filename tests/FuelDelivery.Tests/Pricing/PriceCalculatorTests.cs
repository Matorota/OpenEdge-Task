using System.Globalization;
using FuelDelivery.Domain;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tests.Pricing;

public class PriceCalculatorTests
{
    private readonly PriceCalculator calculator = new(new PricingRules());

    private Shipment Make(
        FuelType type = FuelType.Diesel,
        ClientType client = ClientType.Private,
        string date = "2024-07-04",
        decimal volume = 3000m,
        decimal distance = 100m,
        decimal basePrice = 1m) =>
        new("id", type, client, DateOnly.Parse(date, CultureInfo.InvariantCulture), volume, distance, basePrice);

    [Fact]
    public void Calculates_example_from_task_description()
    {
        DeliveryPrice price = calculator.Calculate(Make(FuelType.Lpg, ClientType.Private, "2024-07-04", 12000m, 90m, 0.51m));

        Assert.Equal(5814.00m, price.NetFuelPrice);
        Assert.Equal(162.00m, price.LogisticsCost);
        Assert.Equal(2160.00m, price.Excise);
        Assert.Equal(1871.28m, price.Vat);
        Assert.Equal(4031.28m, price.TotalTax);
        Assert.Equal(10007.28m, price.GrandTotal);
    }

    [Fact]
    public void Calculates_example_with_custom_rates()
    {
        DeliveryPrice price = calculator.Calculate(
            Make(FuelType.Lpg, ClientType.Private, "2024-07-04", 12000m, 90m, 0.51m), new TaxOverride(0.25m, 0.52m));

        Assert.Equal(4194.00m, price.TotalTax);
        Assert.Equal(10170.00m, price.GrandTotal);
    }

    [Fact]
    public void Applies_discount_before_rounding() =>
        Assert.Equal(5168.48m, calculator.Calculate(Make(volume: 6200m, basePrice: 0.8775m)).NetFuelPrice);

    [Theory]
    [InlineData("0.125", "0.13")]
    [InlineData("2.675", "2.68")]
    public void Rounds_money_half_up(string basePrice, string expected) =>
        Assert.Equal(
            decimal.Parse(expected, CultureInfo.InvariantCulture),
            calculator.Calculate(Make(volume: 1m, basePrice: decimal.Parse(basePrice, CultureInfo.InvariantCulture))).NetFuelPrice);

    [Theory]
    [InlineData(5000, 0)]
    [InlineData(5001, 0.05)]
    [InlineData(20000, 0.05)]
    [InlineData(20001, 0.10)]
    public void Discount_starts_above_limits(int volume, decimal expected) =>
        Assert.Equal(expected, calculator.DiscountRate(volume));

    [Theory]
    [InlineData(999, 330)]
    [InlineData(1000, 180)]
    public void Small_load_fee_applies_under_1000_litres(int volume, decimal expected) =>
        Assert.Equal(expected, calculator.Calculate(Make(volume: volume)).LogisticsCost);

    [Theory]
    [InlineData("2023-12-31", 0.21)]
    [InlineData("2024-01-01", 0.23)]
    public void Private_vat_changes_on_2024_01_01(string date, decimal expected) =>
        Assert.Equal(expected, calculator.VatRate(ClientType.Private, DateOnly.Parse(date, CultureInfo.InvariantCulture)));

    [Fact]
    public void Exempt_client_pays_no_vat_even_with_custom_rate() =>
        Assert.Equal(0m, calculator.Calculate(Make(client: ClientType.Exempt), new TaxOverride(0.25m, 0.52m)).Vat);

    [Fact]
    public void Custom_excise_replaces_only_gasoline()
    {
        var taxOverride = new TaxOverride(0.25m, 0.52m);

        Assert.Equal(0.52m, calculator.ExcisePerLitre(FuelType.Gasoline, taxOverride));
        Assert.Equal(0.33m, calculator.ExcisePerLitre(FuelType.Diesel, taxOverride));
        Assert.Equal(0.18m, calculator.ExcisePerLitre(FuelType.Lpg, taxOverride));
    }
}
