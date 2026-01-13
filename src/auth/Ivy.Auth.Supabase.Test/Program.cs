using System.Globalization;
using Ivy;
using Ivy.Auth.Supabase;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseChrome();
server.UseAuth<SupabaseAuthProvider>(c => c.UseEmailPassword().UseGoogle());
await server.RunAsync();