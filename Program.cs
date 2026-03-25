using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril.Apps.Plans;

var server = new Server();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.Services.AddSingleton<PlanReaderService>();
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(new AppShellSettings().UseTabs(preventDuplicates: true));
await server.RunAsync();
