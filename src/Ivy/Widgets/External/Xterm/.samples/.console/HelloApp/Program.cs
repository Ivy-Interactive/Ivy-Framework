using Spectre.Console;

// Welcome banner
AnsiConsole.Write(
    new FigletText("Hello!")
        .Color(Color.Cyan1));

AnsiConsole.MarkupLine("[grey]Welcome to the Spectre.Console demo![/]");
AnsiConsole.WriteLine();

// Test links
AnsiConsole.MarkupLine("[bold]Useful Links:[/]");
AnsiConsole.MarkupLine("  [grey]>[/] Documentation: [link]https://spectreconsole.net[/]");
AnsiConsole.MarkupLine("  [grey]>[/] GitHub: [link]https://github.com/spectreconsole/spectre.console[/]");
AnsiConsole.MarkupLine("  [grey]>[/] Ivy Framework: [link]https://github.com/Ivy-Interactive/Ivy-Framework[/]");
AnsiConsole.WriteLine();

// Ask for name
var name = AnsiConsole.Ask<string>("[green]What's your [bold]name[/]?[/]");

AnsiConsole.MarkupLine($"Hello, [yellow]{name}[/]!");
AnsiConsole.WriteLine();

// Selection prompt
var fruit = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[blue]What's your [green]favorite fruit[/]?[/]")
        .PageSize(5)
        .AddChoices(new[] { "Apple", "Banana", "Orange", "Mango", "Strawberry" }));

AnsiConsole.MarkupLine($"You selected: [green]{fruit}[/]");
AnsiConsole.WriteLine();

// Multi-selection
var colors = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title("[blue]What are your [green]favorite colors[/]?[/]")
        .PageSize(5)
        .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
        .AddChoices(new[] { "Red", "Green", "Blue", "Yellow", "Purple", "Orange" }));

AnsiConsole.MarkupLine($"You selected: [green]{string.Join(", ", colors)}[/]");
AnsiConsole.WriteLine();

// Confirmation
if (AnsiConsole.Confirm("[yellow]Do you want to see a table?[/]"))
{
    // Table
    var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("[blue]Name[/]")
        .AddColumn("[green]Value[/]")
        .AddColumn("[yellow]Status[/]");

    table.AddRow("CPU", "Intel i9", "[green]OK[/]");
    table.AddRow("Memory", "32 GB", "[green]OK[/]");
    table.AddRow("Disk", "1 TB SSD", "[yellow]Warning[/]");
    table.AddRow("Network", "1 Gbps", "[green]OK[/]");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
}

// Panel
var panel = new Panel(
    new Markup($"[bold]User:[/] {name}\n[bold]Fruit:[/] {fruit}\n[bold]Colors:[/] {string.Join(", ", colors)}"))
    .Header("[blue]Summary[/]")
    .Border(BoxBorder.Rounded)
    .BorderColor(Color.Cyan1);

AnsiConsole.Write(panel);
AnsiConsole.WriteLine();

// Progress bar demo
if (AnsiConsole.Confirm("[yellow]Do you want to see a progress demo?[/]"))
{
    AnsiConsole.Progress()
        .Columns(
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn())
        .Start(ctx =>
        {
            var task1 = ctx.AddTask("[green]Downloading[/]");
            var task2 = ctx.AddTask("[blue]Processing[/]");

            while (!ctx.IsFinished)
            {
                task1.Increment(3);
                task2.Increment(1.5);
                Thread.Sleep(50);
            }
        });

    AnsiConsole.MarkupLine("[green]Done![/]");
}

AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[yellow]Goodbye![/]").RuleStyle("grey"));
AnsiConsole.MarkupLine("\n[grey]Thanks for trying the demo![/]");
