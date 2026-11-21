using FuelDelivery.Domain;

namespace FuelDelivery.Pricing;

/// <summary>Tax rates for one pricing run. Custom values replace the standard ones (Task 2).</summary>
public sealed record TaxRates(decimal? CustomPrivateVat = null, decimal? CustomGasolineExcise = null)
{
    public static readonly TaxRates Standard = new();

    public decimal VatRate(ClientType client, DateOnly date)
    {
        if (client == ClientType.Exempt)
            return Rates.ExemptVat;

        return CustomPrivateVat
            ?? (date < Rates.VatChangeDate ? Rates.PrivateVatBeforeChange : Rates.PrivateVatFromChange);
    }

    public decimal ExcisePerLitre(FuelType type) =>
        type == FuelType.Gasoline && CustomGasolineExcise is { } custom
            ? custom
            : Rates.ExcisePerLitre[type];
}
