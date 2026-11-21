using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tests.Pricing;

public class LateInterestCalculatorTests
{
    private readonly LateInterestCalculator calculator = new(new PricingRules());

    [Fact]
    public void Calculates_example_from_task_description() =>
        Assert.Equal(
            new LatePayment(new DateOnly(2024, 1, 30), 15, 37.96m),
            calculator.Calculate(new DateOnly(2023, 12, 31), 7698.02m, new DateOnly(2024, 2, 14)));

    [Theory]
    [InlineData(30)]
    [InlineData(10)]
    public void Paid_on_time_has_no_interest(int paidDayInJanuary) =>
        Assert.Equal(
            new LatePayment(new DateOnly(2024, 1, 30), 0, 0m),
            calculator.Calculate(new DateOnly(2023, 12, 31), 1000m, new DateOnly(2024, 1, paidDayInJanuary)));

    [Fact]
    public void Unpaid_delivery_is_late_until_report_date_and_splits_at_rate_change()
    {
        // Due 2024-06-25: 5 days at 12% (06-26..06-30) and 20 days at 9% (07-01..07-20).
        LatePayment late = calculator.Calculate(new DateOnly(2024, 5, 26), 1000m, null);

        Assert.Equal(25, late.DaysLate);
        Assert.Equal(1.64m + 4.93m, late.Interest);
    }

    [Fact]
    public void Days_from_rate_change_use_lower_rate() =>
        Assert.Equal(
            new LatePayment(new DateOnly(2024, 6, 30), 5, 1.23m),
            calculator.Calculate(new DateOnly(2024, 5, 31), 1000m, new DateOnly(2024, 7, 5)));

    [Fact]
    public void Delivery_after_report_date_without_payment_is_not_late() =>
        Assert.Equal(0, calculator.Calculate(new DateOnly(2024, 7, 15), 1000m, null).DaysLate);

    [Fact]
    public void Due_date_after_year_9999_does_not_crash() =>
        Assert.Equal(
            new LatePayment(DateOnly.MaxValue, 0, 0m),
            calculator.Calculate(new DateOnly(9999, 12, 31), 1000m, null));
}
