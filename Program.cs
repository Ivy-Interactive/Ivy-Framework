using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril;
using Ivy.Tendril.Services;
using Ivy.Tendril.Apps.Tasks;

var server = new Server();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.Services.AddSingleton<ConfigService>();
server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<PlanReaderService>();
server.Services.AddSingleton<MockTaskDataService>();
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(new AppShellSettings().UseTabs(preventDuplicates: true));
await server.RunAsync();
