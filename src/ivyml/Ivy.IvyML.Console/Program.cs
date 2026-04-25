using Ivy.IvyML.Console;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<DrawCommand>("draw")
        .WithDescription("Render IvyML to a screenshot image.");
    config.AddCommand<DocsCommand>("docs")
        .WithDescription("Show IvyML documentation and widget reference.");
});

return await app.RunAsync(args);
