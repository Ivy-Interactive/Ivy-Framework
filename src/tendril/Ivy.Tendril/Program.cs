using System.ClientModel;
using Ivy;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Ivy.Tendril.AppShell;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;


AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine($"[FATAL] Unhandled exception: {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
    e.SetObserved();
};

var server = new Server();
server.DangerouslyAllowLocalFiles();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.SetMetaTitle("Ivy Tendril");
server.Services.AddSingleton<ConfigService>();

// Register IChatClient if LLM is configured
var configForLlm = new ConfigService();
if (configForLlm.Settings.Llm is { } llmConfig && !string.IsNullOrEmpty(llmConfig.ApiKey))
{
    server.Services.AddSingleton<IChatClient>(sp =>
    {
        var config = sp.GetRequiredService<ConfigService>();
        var llm = config.Settings.Llm!;
        var endpoint = !string.IsNullOrEmpty(llm.Endpoint) ? llm.Endpoint : "https://api.openai.com/v1";
        var client = new OpenAIClient(
            new ApiKeyCredential(llm.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        return client.GetChatClient(llm.Model).AsIChatClient();
    });
}

server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<GitService>();
server.Services.AddSingleton<PlanReaderService>(sp =>
{
    var planService = new PlanReaderService(sp.GetRequiredService<ConfigService>());
    planService.RepairPlans();
    planService.RecoverStuckPlans();
    return planService;
});
server.Services.AddSingleton<TelemetryService>(sp =>
{
    var config = sp.GetRequiredService<ConfigService>();
    return new TelemetryService(config.Settings.Telemetry);
});
server.Services.AddSingleton<JobService>(sp =>
{
    var jobService = new JobService(sp.GetRequiredService<ConfigService>());
    jobService.SetPlanReaderService(sp.GetRequiredService<PlanReaderService>());
    jobService.SetTelemetryService(sp.GetRequiredService<TelemetryService>());
    return jobService;
});
server.Services.AddSingleton<PlanWatcherService>(sp =>
{
    var config = sp.GetRequiredService<ConfigService>();
    return new PlanWatcherService(config);
});
server.Services.AddSingleton<PlanCountsService>(sp =>
{
    var planReader = sp.GetRequiredService<PlanReaderService>();
    var jobService = sp.GetRequiredService<JobService>();
    var planWatcher = sp.GetRequiredService<PlanWatcherService>();
    return new PlanCountsService(planReader, jobService, planWatcher);
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
    app.Services.GetRequiredService<TelemetryService>().TrackAppStarted();
});
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(() => new TendrilAppShell(new AppShellSettings()
    .UseTabs(preventDuplicates: true)));
await server.RunAsync();
