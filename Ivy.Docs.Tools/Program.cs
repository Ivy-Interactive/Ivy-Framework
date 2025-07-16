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
            config.AddCommand<IndexCommand>("index")
                .WithDescription("Generates prompting questions for each markdown file in the documentation.");
            config.PropagateExceptions();
        });

        app.Run(args);
    }
}