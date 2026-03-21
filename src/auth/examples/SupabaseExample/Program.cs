using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<SupabaseExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Supabase Example");

await server.RunAsync();
