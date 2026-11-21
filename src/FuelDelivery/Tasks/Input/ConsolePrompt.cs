using System.Globalization;
using FuelDelivery.Tasks.Parsing;

namespace FuelDelivery.Tasks.Input;

// Asks questions one line at a time and repeats until the answer is valid.
// echoAnswers prints each answer, so piped input still reads well on screen.
public class ConsolePrompt(TextReader input, TextWriter output, DateParser dateParser, bool echoAnswers = false)
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

    public DateOnly AskDate(string question, DateOnly? notBefore = null)
    {
        while (true)
        {
            string answer = Ask(question).Trim();
            if (!dateParser.TryParse(answer, out DateOnly date))
                output.WriteLine("Error: enter a real date as YYYY-MM-DD or DD/MM/YYYY.");
            else if (date < notBefore)
                output.WriteLine($"Error: the date cannot be before {dateParser.Format(notBefore.Value)}.");
            else
                return date;
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
