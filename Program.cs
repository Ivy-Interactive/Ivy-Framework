using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;
using Ivy.Tendril.Views;

var server = new Server();
server.DangerouslyAllowLocalFiles();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.SetMetaTitle("Ivy Tendril");
server.Services.AddSingleton<ConfigService>();
server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<PlanReaderService>();
server.Services.AddSingleton<JobService>();
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .Footer(new NewPlanFooterButton()));
await server.RunAsync();
