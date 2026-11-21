namespace FuelDelivery.Tasks.Pricing;

/// <summary>Custom rates from Task 2. VAT replaces both private rates, excise replaces only gasoline.</summary>
public record TaxOverride(decimal PrivateVat, decimal GasolineExcise);
