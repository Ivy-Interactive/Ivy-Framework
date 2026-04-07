using Ivy.Tendril;
using Ivy.Tendril.Database;

// Handle database CLI commands before starting the server
var dbExitCode = DatabaseCommands.Handle(args);
if (dbExitCode >= 0)
    return dbExitCode;

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
    e.SetObserved();
};

var server = TendrilServer.Create(args);
await server.RunAsync();
return 0;
