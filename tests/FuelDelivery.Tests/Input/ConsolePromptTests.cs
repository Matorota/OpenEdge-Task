using FuelDelivery.Tasks.Input;
using FuelDelivery.Tasks.Parsing;

namespace FuelDelivery.Tests.Input;

public class ConsolePromptTests
{
    private ConsolePrompt Prompt(string input, StringWriter? output = null) =>
        new(new StringReader(input), output ?? new StringWriter(), new DateParser());

    [Theory]
    [InlineData("25\n", 25)]
    [InlineData(" 0.52 \n", 0.52)]
    [InlineData("0\n", 0)]
    [InlineData("abc\n-1\n1,5\n\n7.5\n", 7.5)]
    public void Asks_number_again_until_valid(string input, decimal expected) =>
        Assert.Equal(expected, Prompt(input).AskNonNegativeNumber("?"));

    [Fact]
    public void Ignores_byte_order_mark_from_powershell() =>
        Assert.Equal(25, Prompt(ConsolePrompt.ByteOrderMark + "25\r\n").AskNonNegativeNumber("?"));

    [Fact]
    public void Shows_error_for_invalid_number()
    {
        var output = new StringWriter();
        Prompt("-3\n4\n", output).AskNonNegativeNumber("Enter:");

        Assert.Contains("Error", output.ToString());
    }

    [Fact]
    public void Asks_date_again_until_valid_and_not_before_limit() =>
        Assert.Equal(
            new DateOnly(2024, 1, 31),
            Prompt("2024-02-30\n2023-12-31\n31/01/2024\n").AskDate("?", new DateOnly(2024, 1, 1)));

    [Fact]
    public void Throws_when_input_ends() =>
        Assert.Throws<EndOfStreamException>(() => Prompt("abc\n").AskNonNegativeNumber("?"));
}
