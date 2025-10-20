using System.Diagnostics;
using Ivy.Filters;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ivy.Filters.Eval.Console;

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<testFile>")]
        public string TestFile { get; init; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.TestFile))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Test file not found: {settings.TestFile}");
            return 1;
        }

        // Read test document
        var testDoc = TestReader.Read(settings.TestFile);

        if (testDoc.Models.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] No models specified in test file");
            return 1;
        }

        if (testDoc.Suites.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] No test suites found in test file");
            return 1;
        }

        // Load configuration from user secrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RunCommand>()
            .Build();

        var endpoint = configuration["OpenAi:Endpoint"] ?? throw new InvalidOperationException("OpenAi:Endpoint not found in user secrets");
        var apiKey = configuration["OpenAi:ApiKey"] ?? throw new InvalidOperationException("OpenAi:ApiKey not found in user secrets");

        // Initialize cost service
        var httpClient = new HttpClient();
        var costService = new ModelCostService(httpClient);
        var costsLoaded = await costService.LoadModelCostsFromLiteLLMAsync(apiKey);

        if (!costsLoaded)
        {
            AnsiConsole.MarkupLine("[yellow]Warning:[/] Could not load model costs from LiteLLM API, using fallback pricing");
        }

        // Results collection
        var modelResults = new List<ModelResult>();

        // Run tests for each model
        foreach (var modelName in testDoc.Models.Where(m => !string.IsNullOrWhiteSpace(m)))
        {
            AnsiConsole.MarkupLine($"\n[bold blue]Testing model:[/] {modelName}");

            var chatClient = CreateChatClient(modelName, apiKey, endpoint);
            if (chatClient == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Skipping unsupported model: {modelName}");
                continue;
            }

            var logger = new ConsoleLogger();
            var agent = new FilterParserAgent(chatClient, logger);

            var testResults = new List<TestResult>();
            var totalTime = 0.0;
            long totalInputTokens = 0;
            long totalOutputTokens = 0;
            int totalIterations = 0;

            // Run all tests across all suites
            foreach (var suite in testDoc.Suites)
            {
                AnsiConsole.MarkupLine($"  [dim]Running suite:[/] {suite.Name}");

                foreach (var test in suite.Tests)
                {
                    var sw = Stopwatch.StartNew();
                    var result = await agent.Parse(test.Filter, suite.Fields);
                    sw.Stop();

                    var timeMs = sw.Elapsed.TotalMilliseconds;
                    totalTime += timeMs;

                    // Track token usage and iterations
                    if (result.Usage != null)
                    {
                        totalInputTokens += result.Usage.InputTokens;
                        totalOutputTokens += result.Usage.OutputTokens;
                    }
                    totalIterations += result.Iterations;

                    var success = EvaluateResult(result, test.Expected);
                    testResults.Add(new TestResult(
                        test.Filter,
                        success,
                        timeMs,
                        result.HasErrors ? string.Join("; ", result.Diagnostics.Select(d => d.Message)) : null
                    ));

                    // Show progress
                    var icon = success ? "[green]✓[/]" : "[red]✗[/]";
                    AnsiConsole.MarkupLine($"    {icon} {test.Filter.EscapeMarkup()}");

                    // Print detailed failure info
                    if (!success)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine($"      [red]Model:[/] {modelName.EscapeMarkup()}");
                        AnsiConsole.MarkupLine($"      [red]Suite:[/] {suite.Name.EscapeMarkup()}");
                        AnsiConsole.MarkupLine($"      [red]Test:[/] {test.Filter.EscapeMarkup()}");
                        AnsiConsole.MarkupLine($"      [red]Expected:[/]");
                        foreach (var exp in test.Expected)
                        {
                            AnsiConsole.MarkupLine($"        - {exp.EscapeMarkup()}");
                        }
                        AnsiConsole.MarkupLine($"      [red]Result:[/]");
                        if (result.HasErrors)
                        {
                            AnsiConsole.MarkupLine($"        ERROR: {string.Join("; ", result.Diagnostics.Select(d => d.Message)).EscapeMarkup()}");
                        }
                        else
                        {
                            var filterText = AstFormatter.ToFilterExpression(result.Ast);
                            AnsiConsole.MarkupLine($"        {filterText.EscapeMarkup()}");
                        }
                        AnsiConsole.WriteLine();
                    }
                }
            }

            var passedCount = testResults.Count(r => r.Success);
            var totalCount = testResults.Count;
            var successRate = totalCount > 0 ? (double)passedCount / totalCount * 100 : 0;
            var avgTime = totalCount > 0 ? totalTime / totalCount : 0;
            var avgIterations = totalCount > 0 ? (double)totalIterations / totalCount : 0;

            // Calculate total cost
            var totalCost = costService.CalculateCost(modelName, totalInputTokens, totalOutputTokens);

            modelResults.Add(new ModelResult(
                modelName,
                passedCount,
                totalCount,
                successRate,
                avgTime,
                totalTime,
                totalInputTokens,
                totalOutputTokens,
                totalCost,
                avgIterations
            ));
        }

        // Display results table
        DisplayResultsTable(modelResults);

        return 0;
    }

    private IChatClient? CreateChatClient(string modelName, string apiKey, string endpoint)
    {
        // All models go through LiteLLM proxy
        var openAiClientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var credential = new System.ClientModel.ApiKeyCredential(apiKey);
        var openAiClient = new OpenAIClient(credential, openAiClientOptions);
        var openAIChatClient = openAiClient.GetChatClient(modelName);
        return openAIChatClient.AsIChatClient();
    }

    private bool EvaluateResult(FilterParseResult result, string[] expected)
    {
        // If expected contains ERROR, check if result has errors
        if (expected.Any(e => e.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase)))
        {
            return result.HasErrors;
        }

        // If result has errors but we didn't expect any, it's a failure
        if (result.HasErrors)
        {
            return false;
        }

        // Result succeeded - in a full implementation, we'd compare the AST or generated formula
        // For now, we just check that parsing succeeded
        return result.Ast != null;
    }

    private void DisplayResultsTable(List<ModelResult> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Test Results[/]"));
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("Model");
        table.AddColumn("Passed");
        table.AddColumn("Total");
        table.AddColumn("Success Rate");
        table.AddColumn("Avg Time (ms)");
        table.AddColumn("Total Time (s)");
        table.AddColumn("Avg Iterations");
        table.AddColumn("Input Tokens");
        table.AddColumn("Output Tokens");
        table.AddColumn("Total Cost");

        foreach (var result in results.OrderByDescending(r => r.SuccessRate).ThenBy(r => r.AvgTimeMs))
        {
            var successColor = result.SuccessRate >= 90 ? "green" :
                             result.SuccessRate >= 70 ? "yellow" : "red";

            var costColor = result.TotalCost < 0.10m ? "green" :
                           result.TotalCost < 0.50m ? "yellow" : "red";

            var iterationColor = result.AvgIterations <= 1.5 ? "green" :
                                result.AvgIterations <= 2.5 ? "yellow" : "red";

            table.AddRow(
                result.ModelName,
                result.PassedCount.ToString(),
                result.TotalCount.ToString(),
                $"[{successColor}]{result.SuccessRate:F1}%[/]",
                $"{result.AvgTimeMs:F0}",
                $"{result.TotalTimeMs / 1000:F1}",
                $"[{iterationColor}]{result.AvgIterations:F2}[/]",
                result.TotalInputTokens.ToString("N0"),
                result.TotalOutputTokens.ToString("N0"),
                $"[{costColor}]${result.TotalCost:F4}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private record ModelResult(
        string ModelName,
        int PassedCount,
        int TotalCount,
        double SuccessRate,
        double AvgTimeMs,
        double TotalTimeMs,
        long TotalInputTokens,
        long TotalOutputTokens,
        decimal TotalCost,
        double AvgIterations
    );

    private record TestResult(
        string Filter,
        bool Success,
        double TimeMs,
        string? ErrorMessage
    );

    private class ConsoleLogger : ILogger<FilterParserAgent>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var color = logLevel switch
            {
                LogLevel.Error or LogLevel.Critical => "red",
                LogLevel.Warning => "yellow",
                _ => "dim"
            };

            AnsiConsole.MarkupLine($"      [{color}]{logLevel.ToString().EscapeMarkup()}:[/] {message.EscapeMarkup()}");

            if (exception != null)
            {
                AnsiConsole.MarkupLine($"      [{color}]Exception: {exception.Message.EscapeMarkup()}[/]");
            }
        }
    }
}
