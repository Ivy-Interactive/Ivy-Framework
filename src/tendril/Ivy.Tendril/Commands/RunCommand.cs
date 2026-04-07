using Ivy.Tendril.Database;
using Ivy.Tendril.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ivy.Tendril.Commands;

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    private readonly IServiceProvider _serviceProvider;

    public RunCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-p|--port")]
        public int? Port { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var config = _serviceProvider.GetRequiredService<IConfigService>();
        var dbPath = Path.Combine(config.TendrilHome, "tendril.db");

        // Ensure directory exists
        var dbDir = Path.GetDirectoryName(dbPath);
        if (dbDir != null && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        AnsiConsole.MarkupLine("[blue]Checking database status...[/]");

        try
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var migrator = new DatabaseMigrator(connection);
            var current = migrator.GetCurrentVersion();
            var latest = migrator.GetLatestVersion();

            if (current < latest)
            {
                AnsiConsole.MarkupLine($"[yellow]Migrating database from version {current} to {latest}...[/]");
                migrator.ApplyMigrations();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Database migration failed: {ex.Message}[/]");
            return 1;
        }

        AnsiConsole.MarkupLine("[green]Starting Ivy Tendril server on localhost:5010...[/]");

        var server = TendrilServer.Create([]);
        if (settings.Port.HasValue)
        {
            server.Args.Port = settings.Port.Value;
        }

        await server.RunAsync();

        return 0;
    }
}
