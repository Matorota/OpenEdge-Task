# Fuel Delivery Pricing

C# (.NET 8) console program that prices fuel deliveries from `shipments.json` and writes CSV reports to `output/`.

**Parts done:** Task 1, Task 2, Bonus 1, Bonus 2.

## How to run

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Get the code and open a terminal in its folder:

```
git clone https://github.com/Matorota/OpenEdge-Task-.git
cd OpenEdge-Task-
```

3. Run (no arguments needed):

```
dotnet run --project src/FuelDelivery
```

The program reads `shipments.json` and `payments.json` from the folder where it is started and writes the
`output` folder there. To use other data, replace the two JSON files, or start it from the data folder:
`dotnet run --project <path to this repo>/src/FuelDelivery`.

The program asks, one line each: Task 2 VAT (%), Task 2 gasoline excise, Bonus 1 start date, Bonus 1 end date.
It ends by itself after the last answer.

```
PowerShell:    "25","0.52","2024-01-01","2024-01-31" | dotnet run --project src/FuelDelivery
Linux/macOS:   printf "25\n0.52\n2024-01-01\n2024-01-31\n" | dotnet run --project src/FuelDelivery
```

Optional, to run one part: add `-- task1`, `-- task2`, `-- bonus1` or `-- bonus2` to the command.
Tests: `dotnet test`

Output files: `errors.csv`, `task1.csv`, `task2.csv`, `bonus1.csv`, `bonus2.csv`.

## Program.cs

The entry point. It creates all objects once, reads `shipments.json`, writes `errors.csv`,
then runs Task 1 → Bonus 2 → Task 2 → Bonus 1. Bonus 2 asks nothing, so it runs before the questions.

## Project structure

```
src/FuelDelivery/
  Program.cs     Entry point
  Domain/        Shipment, FuelType, ClientType
  Tasks/
    Pricing/     PricingRules (all rates and dates), PriceCalculator, LateInterestCalculator
    Parsing/     ShipmentReader (validation), PaymentReader, DateParser
    Input/       ConsolePrompt (asks again until the answer is valid)
    Output/      CsvWriter, PriceReport, ErrorReport
    Task1/ Task2/ Bonus1/ Bonus2/
tests/           Unit tests, including every example from the task
```

- Money is `decimal`, rounded half up to 2 decimals at every step.
- All rates are in `PricingRules`, away from file reading and console input.
- `PriceCalculator` prices one delivery without files or console.

## Date formats

Only `YYYY-MM-DD` and `DD/MM/YYYY` are accepted. A regular expression checks the exact format,
then the date is checked to be real (`2024-02-29` is valid, `2023-02-30` is not).
The same rules are used for shipment dates, payment dates and the Bonus 1 answers.
Output dates are always `YYYY-MM-DD`.

## Assumptions

- One error code per record, the first problem wins, in this order: missing id, duplicate id, missing field,
  unknown type, unknown client, invalid date, invalid value.
- Numbers above 1,000,000,000 are treated as `INVALID_VALUE`, because they are not realistic and would overflow the calculation.
- A missing or broken `payments.json` means no delivery has been paid.
- Wrong answers (text, negative numbers, invalid dates, end before start) show an error and are asked again.
- Bonus 2: when late days cross 2024-07-01, the 12% and 9% parts are rounded separately, then added.

## AI usage

I used **Claude Code** as a helping tool for writing code and for testing. I led the work and made the decisions.

- **My decisions and work:** chose C#; designed the structure and code style (object-oriented classes with
  constructor injection instead of static code, all rates in one place, the `Tasks` folder layout, a separate
  `DeliveryPrice` result, simple comments); reviewed and refactored the code task by task; ran the program and
  checked every result against the task examples; managed the work in git with branches and pull requests.
- **AI as a helper:** wrote code drafts from my instructions, wrote unit tests, and tested the program with
  invalid data, which found edge cases such as the PowerShell byte order mark and dates near year 9999.
