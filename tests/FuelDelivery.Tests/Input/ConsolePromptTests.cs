using FuelDelivery.Tasks.Input;

namespace FuelDelivery.Tests.Input;

public class ConsolePromptTests
{
    private ConsolePrompt Prompt(string input, StringWriter? output = null) =>
        new(new StringReader(input), output ?? new StringWriter());

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
    public void Throws_when_input_ends() =>
        Assert.Throws<EndOfStreamException>(() => Prompt("abc\n").AskNonNegativeNumber("?"));
}
