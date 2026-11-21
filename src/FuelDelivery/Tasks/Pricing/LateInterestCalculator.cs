namespace FuelDelivery.Tasks.Pricing;

public record LatePayment(DateOnly DueDate, int DaysLate, decimal Interest);

// Interest for paying after the due date. Each rate period is rounded on its own.
public class LateInterestCalculator(PricingRules rules)
{
    public LatePayment Calculate(DateOnly deliveryDate, decimal grandTotal, DateOnly? paidDate)
    {
        // A due date after year 9999 cannot be stored, so it is capped; such a delivery is never late anyway.
        DateOnly dueDate = DateOnly.FromDayNumber(
            Math.Min(deliveryDate.DayNumber + rules.PaymentTermDays, DateOnly.MaxValue.DayNumber));
        DateOnly lastDay = paidDate ?? rules.ReportDate;
        int daysLate = Math.Max(0, lastDay.DayNumber - dueDate.DayNumber);

        // Late days start the day after the due date; split them at the rate change.
        int firstLateDay = dueDate.DayNumber + 1;
        int daysBeforeChange = Math.Clamp(rules.InterestChangeDate.DayNumber - firstLateDay, 0, daysLate);
        int daysFromChange = daysLate - daysBeforeChange;

        decimal interest = PeriodInterest(grandTotal, rules.InterestBeforeChange, daysBeforeChange)
                           + PeriodInterest(grandTotal, rules.InterestFromChange, daysFromChange);

        return new LatePayment(dueDate, daysLate, interest);
    }

    // Multiply by the days first, then round half up.
    private decimal PeriodInterest(decimal grandTotal, decimal yearlyRate, int days) =>
        Math.Round(grandTotal * yearlyRate * days / rules.DaysPerYear, 2, MidpointRounding.AwayFromZero);
}
