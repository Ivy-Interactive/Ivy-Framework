using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<SupabaseExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Supabase Example");

Server.AuthCookiePrefix = "supabase";

await server.RunAsync();
