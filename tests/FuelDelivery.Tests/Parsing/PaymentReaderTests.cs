using FuelDelivery.Tasks.Parsing;

namespace FuelDelivery.Tests.Parsing;

public class PaymentReaderTests
{
    private readonly PaymentReader reader = new(new DateParser());

    [Fact]
    public void Keeps_first_usable_payment_and_ignores_the_rest()
    {
        const string json = """
            [
              { "id": "a", "paidDate": "2024-02-30" },
              { "id": "a", "paidDate": "14/02/2024" },
              { "id": "a", "paidDate": "2024-01-01" },
              { "id": "A", "paidDate": "2024-01-01" },
              { "id": "unknown", "paidDate": "2024-01-01" },
              { "id": "b", "paidDate": null },
              { "paidDate": "2024-01-01" },
              { "id": "b", "paidDate": 20240101 },
              42
            ]
            """;

        Dictionary<string, DateOnly> payments = reader.Parse(json, new HashSet<string> { "a", "b" });

        Assert.Equal(new DateOnly(2024, 2, 14), Assert.Single(payments).Value);
        Assert.DoesNotContain("b", payments.Keys);
    }
}
