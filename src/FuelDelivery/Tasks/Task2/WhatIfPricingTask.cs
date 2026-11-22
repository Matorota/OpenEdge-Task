using FuelDelivery.Domain;
using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Output;
using FuelDelivery.Tasks.Pricing;

namespace FuelDelivery.Tasks.Task2;

// Task 2: prices every delivery again with a custom private VAT and gasoline excise.
public class WhatIfPricingTask(PriceCalculator calculator, PriceReport report, ConsolePrompt prompt)
{
    public List<(Shipment Shipment, DeliveryPrice Price)> Run(IReadOnlyList<Shipment> shipments)
    {
        decimal vatPercent = prompt.AskNonNegativeNumber("Enter custom VAT rate for Private clients (%):");
        decimal gasolineExcise = prompt.AskNonNegativeNumber("Enter custom Excise rate for Gasoline:");

        // The user types 25 for 25%, so divide by 100.
        var taxOverride = new TaxOverride(vatPercent / 100m, gasolineExcise);
        var pricedShipments = shipments.Select(s => (s, calculator.Calculate(s, taxOverride))).ToList();
        report.Write("task2.csv", pricedShipments);
        return pricedShipments;
    }
}
