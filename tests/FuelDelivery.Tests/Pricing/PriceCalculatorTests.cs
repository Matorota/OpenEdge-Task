using System.Globalization;
using FuelDelivery.Domain;
using FuelDelivery.Pricing;

namespace FuelDelivery.Tests.Pricing;

public class PriceCalculatorTests
{
    private static Shipment Make(
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
        var invoice = PriceCalculator.Calculate(
            Make(FuelType.Lpg, ClientType.Private, "2024-07-04", 12000m, 90m, 0.51m), TaxRates.Standard);

        Assert.Equal(5814.00m, invoice.NetFuelPrice);
        Assert.Equal(162.00m, invoice.LogisticsCost);
        Assert.Equal(2160.00m, invoice.Excise);
        Assert.Equal(1871.28m, invoice.Vat);
        Assert.Equal(4031.28m, invoice.TotalTax);
        Assert.Equal(10007.28m, invoice.GrandTotal);
    }

    [Fact]
    public void Applies_discount_before_rounding()
    {
        var invoice = PriceCalculator.Calculate(Make(volume: 6200m, basePrice: 0.8775m), TaxRates.Standard);

        Assert.Equal(5168.48m, invoice.NetFuelPrice);
    }

    [Theory]
    [InlineData(5000, 0)]
    [InlineData(5001, 0.05)]
    [InlineData(20000, 0.05)]
    [InlineData(20001, 0.10)]
    public void Discount_starts_above_limits(int volume, decimal expected) =>
        Assert.Equal(expected, PriceCalculator.DiscountRate(volume));

    [Theory]
    [InlineData(999, 330)]
    [InlineData(1000, 180)]
    public void Small_load_fee_applies_under_1000_litres(int volume, decimal expected) =>
        Assert.Equal(expected, PriceCalculator.Calculate(Make(volume: volume), TaxRates.Standard).LogisticsCost);

    [Theory]
    [InlineData("2023-12-31", 0.21)]
    [InlineData("2024-01-01", 0.23)]
    public void Private_vat_changes_on_2024_01_01(string date, decimal expected) =>
        Assert.Equal(expected, TaxRates.Standard.VatRate(ClientType.Private, DateOnly.Parse(date, CultureInfo.InvariantCulture)));

    [Fact]
    public void Exempt_client_pays_no_vat() =>
        Assert.Equal(0m, PriceCalculator.Calculate(Make(client: ClientType.Exempt), TaxRates.Standard).Vat);

    [Theory]
    [InlineData("0.125", "0.13")]
    [InlineData("2.675", "2.68")]
    [InlineData("1234567.005", "1234567.01")]
    public void Money_rounds_half_up_and_formats_invariant(string value, string expected) =>
        Assert.Equal(expected, Money.Format(Money.Round(decimal.Parse(value, CultureInfo.InvariantCulture))));
}
