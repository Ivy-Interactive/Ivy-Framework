using Ivy.IvyML.Console;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<DrawCommand>("draw")
        .WithDescription("Render IvyML to a screenshot image.");
    config.AddCommand<ParseCommand>("parse")
        .WithDescription("Verify that IvyML parses correctly, without rendering.");
    config.AddCommand<DocsCommand>("docs")
        .WithDescription("Show IvyML documentation and widget reference.");
    config.AddCommand<IconsCommand>("icons")
        .WithDescription("Search for icons by name.");
});

return await app.RunAsync(args);
