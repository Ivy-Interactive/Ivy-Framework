using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<ClerkExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Clerk Example");

await server.RunAsync();
