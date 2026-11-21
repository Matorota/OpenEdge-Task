using FuelDelivery.Tasks.Parsing;

namespace FuelDelivery.Tests.Parsing;

public class DateParserTests
{
    private readonly DateParser parser = new();

    [Theory]
    [InlineData("2024-02-29", 2024, 2, 29)]
    [InlineData("15/01/2024", 2024, 1, 15)]
    [InlineData("31/12/2023", 2023, 12, 31)]
    public void Parses_both_formats(string text, int year, int month, int day)
    {
        Assert.True(parser.TryParse(text, out DateOnly date));
        Assert.Equal(new DateOnly(year, month, day), date);
    }

    [Theory]
    [InlineData("2023-02-30")]
    [InlineData("2023-02-29")]
    [InlineData("2024-13-01")]
    [InlineData("2024-00-10")]
    [InlineData("0000-01-01")]
    [InlineData("32/01/2024")]
    [InlineData("2024/01/15")]
    [InlineData("01-15-2024")]
    [InlineData("2024-1-5")]
    [InlineData("1/1/2024")]
    [InlineData(" 2024-01-01")]
    [InlineData("2024-01-01\n")]
    [InlineData("2024-01-01T00:00:00")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_invalid_dates(string? text) =>
        Assert.False(parser.TryParse(text, out _));
}
