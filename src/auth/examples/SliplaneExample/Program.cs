using Ivy.Auth.Sliplane;

var server = new Server();

server.UseHotReload();

server.UseAuth<SliplaneAuthProvider>();

server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<SliplaneExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Sliplane Example");

await server.RunAsync();

