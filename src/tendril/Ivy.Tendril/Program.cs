using Ivy.Tendril.Commands;
using Ivy.Tendril.Database;
using Ivy.Tendril.Infrastructure;
using Ivy.Tendril.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Text;

namespace Ivy.Tendril;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Restore original database command handling
        var dbExitCode = DatabaseCommands.Handle(args);
        if (dbExitCode >= 0)
        {
            return dbExitCode;
        }

        if (!Console.IsOutputRedirected)
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);

        var registrar = new TypeRegistrar(services);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("tendril");
            config.AddCommand<RunCommand>("run")
                .WithDescription("Start the Tendril web server on localhost:5010");
            config.AddCommand<VersionCommand>("version")
                .WithDescription("Show the current version of Tendril");
        });

        return await app.RunAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        var configService = new ConfigService();
        services.AddSingleton<IConfigService>(configService);
        services.AddSingleton<ConfigService>(configService);

        var telemetryService = new TelemetryService(configService.Settings.Telemetry, new Logger<TelemetryService>(new LoggerFactory()));
        services.AddSingleton<ITelemetryService>(telemetryService);

        // Add internal services for Commands
        services.AddSingleton<DatabaseMigrator>(sp =>
        {
            var config = sp.GetRequiredService<IConfigService>();
            var dbPath = Path.Combine(config.TendrilHome, "tendril.db");
            var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            return new DatabaseMigrator(connection);
        });
    }
}
