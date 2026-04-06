using Ivy.Docs.Tools;
using Spectre.Console.Cli;

public static class Program
{
    static void Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("Ivy.Docs.Tools");
            config.AddCommand<ConvertCommand>("convert")
                .WithDescription("Converts markdown files to Ivy C# App.");
            config.AddCommand<GenerateApiDocsCommand>("generate-api-docs")
                .WithDescription("Generates a JSON manifest of API documentation for WidgetDocs tags.");
            config.PropagateExceptions();
        });

        app.Run(args);
    }
}