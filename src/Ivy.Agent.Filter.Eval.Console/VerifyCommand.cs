using Ivy.Filters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ivy.Filters.Eval.Console;

public class VerifyCommand : Command<VerifyCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<testFile>")]
        public string TestFile { get; init; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.TestFile))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Test file not found: {settings.TestFile}");
            return 1;
        }

        // Read test document
        var testDoc = TestReader.Read(settings.TestFile);

        if (testDoc.Suites.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] No test suites found in test file");
            return 1;
        }

        AnsiConsole.Write(new Rule("[bold yellow]Verifying Test Expectations[/]"));
        AnsiConsole.WriteLine();

        var totalTests = 0;
        var totalExpectations = 0;
        var validExpectations = 0;
        var invalidExpectations = 0;
        var errorExpectations = 0; // Expected errors

        foreach (var suite in testDoc.Suites)
        {
            AnsiConsole.MarkupLine($"\n[bold blue]Suite:[/] {suite.Name}");

            var parser = new FilterParser(suite.Fields);

            foreach (var test in suite.Tests)
            {
                totalTests++;

                foreach (var expected in test.Expected)
                {
                    totalExpectations++;

                    // Check if this is an expected error
                    if (expected.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                    {
                        errorExpectations++;
                        AnsiConsole.MarkupLine($"  [dim]✓ {test.Filter.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"    [dim]Expected error: {expected.EscapeMarkup()}[/]");
                        continue;
                    }

                    // Parse the expected formula
                    var result = parser.Parse(expected);

                    if (result.HasErrors)
                    {
                        invalidExpectations++;
                        AnsiConsole.MarkupLine($"  [red]✗ {test.Filter.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"    [red]Invalid expected formula:[/] {expected.EscapeMarkup()}");

                        foreach (var diagnostic in result.Diagnostics)
                        {
                            AnsiConsole.MarkupLine($"      [red]• {diagnostic.Message.EscapeMarkup()}[/]");
                        }
                    }
                    else
                    {
                        validExpectations++;
                        AnsiConsole.MarkupLine($"  [green]✓ {test.Filter.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"    [dim]{expected.EscapeMarkup()}[/]");
                    }
                }
            }
        }

        // Display summary
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Verification Summary[/]"));
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("Metric");
        table.AddColumn("Count");

        table.AddRow("Total Tests", totalTests.ToString());
        table.AddRow("Total Expectations", totalExpectations.ToString());
        table.AddRow("[green]Valid Formulas[/]", validExpectations.ToString());
        table.AddRow("[red]Invalid Formulas[/]", invalidExpectations.ToString());
        table.AddRow("[dim]Expected Errors[/]", errorExpectations.ToString());

        AnsiConsole.Write(table);

        if (invalidExpectations > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]⚠ Found {invalidExpectations} invalid expected formula(s). Please fix the test file.[/]");
            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]✓ All expected formulas are valid![/]");
        return 0;
    }
}
