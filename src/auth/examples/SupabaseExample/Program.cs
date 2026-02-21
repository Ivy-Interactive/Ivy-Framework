using Ivy;
using Ivy.Auth.Supabase;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<SupabaseAuthProvider>(auth => auth
    .UseEmailPassword()
    .UseGoogle());

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<SupabaseExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Supabase Example");

await server.RunAsync();
