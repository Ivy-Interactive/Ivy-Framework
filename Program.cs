using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril.AppShell;
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
server.Services.AddSingleton<GitService>();
server.Services.AddSingleton<PlanReaderService>(sp =>
{
    var planService = new PlanReaderService(sp.GetRequiredService<ConfigService>());
    planService.RepairPlans();
    planService.RecoverStuckPlans();
    return planService;
});
server.Services.AddSingleton<JobService>(sp =>
{
    var jobService = new JobService();
    jobService.SetPlanReaderService(sp.GetRequiredService<PlanReaderService>());
    return jobService;
});
server.Services.AddSingleton<PlanWatcherService>(sp =>
{
    var config = sp.GetRequiredService<ConfigService>();
    return new PlanWatcherService(config);
});
server.Services.AddSingleton<InboxWatcherService>(sp =>
{
    var config = sp.GetRequiredService<ConfigService>();
    var jobService = sp.GetRequiredService<JobService>();
    return new InboxWatcherService(config, jobService);
});
server.UseWebApplication(app =>
{
    // Eagerly resolve watcher services so their FileSystemWatchers start immediately
    app.Services.GetRequiredService<PlanWatcherService>();
    app.Services.GetRequiredService<InboxWatcherService>();
});
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(() => new TendrilAppShell(new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .Footer(new NewPlanFooterButton())));
await server.RunAsync();
