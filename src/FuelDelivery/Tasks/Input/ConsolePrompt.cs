using System.Globalization;

namespace FuelDelivery.Tasks.Input;

/// <summary>Asks questions one line at a time and repeats until the answer is valid.</summary>
/// <param name="echoAnswers">Prints each answer, so piped input still reads well on screen.</param>
public class ConsolePrompt(TextReader input, TextWriter output, bool echoAnswers = false)
{
    private const NumberStyles NumberFormat =
        NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    // Windows PowerShell adds a byte order mark to the first piped line.
    public const char ByteOrderMark = (char)0xFEFF;

    public decimal AskNonNegativeNumber(string question)
    {
        while (true)
        {
            string answer = Ask(question);
            if (decimal.TryParse(answer, NumberFormat, CultureInfo.InvariantCulture, out decimal value) && value >= 0)
                return value;

            output.WriteLine("Error: enter a number that is 0 or greater, for example 25 or 0.52.");
        }
    }

    private string Ask(string question)
    {
        output.Write(question + " ");
        string line = input.ReadLine() ?? throw new EndOfStreamException("Input ended before all answers were given.");
        string answer = line.TrimStart(ByteOrderMark);
        if (echoAnswers)
            output.WriteLine(answer);
        return answer;
    }
}
