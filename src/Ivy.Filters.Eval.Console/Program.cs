using Ivy.Filters.Eval.Console;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();
var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<RunCommand>("run")
        .WithDescription("Run filter parsing tests from a YAML test file");

    config.AddCommand<VerifyCommand>("verify")
        .WithDescription("Verify that all expected formulas in a test file are valid");
});

return await app.RunAsync(args);
