using Ivy;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril.AppShell;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;


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
var configService = new ConfigService();
server.Services.AddSingleton<IConfigService>(configService);
server.Services.AddSingleton<ConfigService>(configService);
var modelPricingService = new ModelPricingService();
server.Services.AddSingleton<IModelPricingService>(modelPricingService);
server.Services.AddSingleton<ModelPricingService>(modelPricingService);

// Register IChatClient if LLM is configured
if (configService.Settings.Llm is { } llmConfig && !string.IsNullOrEmpty(llmConfig.ApiKey))
{
    server.Services.AddSingleton<IChatClient>(sp =>
    {
        var config = sp.GetRequiredService<IConfigService>();
        var llm = config.Settings.Llm!;
        var endpoint = !string.IsNullOrEmpty(llm.Endpoint) ? llm.Endpoint : "https://api.openai.com/v1";
        var client = new OpenAIClient(
            new ApiKeyCredential(llm.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        return client.GetChatClient(llm.Model).AsIChatClient();
    });
}

server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<IGithubService>(sp => sp.GetRequiredService<GithubService>());
server.Services.AddSingleton<GitService>();
server.Services.AddSingleton<IGitService>(sp => sp.GetRequiredService<GitService>());
server.Services.AddSingleton<PlanReaderService>(sp =>
{
    var planService = new PlanReaderService(sp.GetRequiredService<IConfigService>());
    planService.RepairPlans();
    planService.RecoverStuckPlans();
    return planService;
});
server.Services.AddSingleton<IPlanReaderService>(sp => sp.GetRequiredService<PlanReaderService>());

// SQLite database service
server.Services.AddSingleton<IPlanDatabaseService>(sp =>
{
    var cfg = sp.GetRequiredService<IConfigService>();
    var dbPath = Path.Combine(cfg.TendrilHome, "tendril.db");
    return new PlanDatabaseService(dbPath);
});
server.Services.AddSingleton<PlanDatabaseSyncService>(sp =>
{
    var planReader = sp.GetRequiredService<PlanReaderService>();
    var database = sp.GetRequiredService<IPlanDatabaseService>();
    var watcher = sp.GetRequiredService<IPlanWatcherService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new PlanDatabaseSyncService(planReader, database, watcher,
        loggerFactory.CreateLogger<PlanDatabaseSyncService>());
});
server.Services.AddSingleton<ITelemetryService>(sp =>
{
    var config = sp.GetRequiredService<IConfigService>();
    return new TelemetryService(config.Settings.Telemetry);
});
server.Services.AddSingleton<TelemetryService>(sp =>
    (TelemetryService)sp.GetRequiredService<ITelemetryService>());
server.Services.AddSingleton<JobService>(sp =>
{
    return new JobService(
        sp.GetRequiredService<IConfigService>(),
        sp.GetRequiredService<ModelPricingService>(),
        sp.GetRequiredService<IPlanReaderService>(),
        sp.GetRequiredService<ITelemetryService>());
});
server.Services.AddSingleton<IJobService>(sp => sp.GetRequiredService<JobService>());
server.Services.AddSingleton<PlanWatcherService>(sp =>
{
    var config = sp.GetRequiredService<IConfigService>();
    return new PlanWatcherService(config);
});
server.Services.AddSingleton<IPlanWatcherService>(sp => sp.GetRequiredService<PlanWatcherService>());
server.Services.AddSingleton<PlanCountsService>(sp =>
{
    var planReader = sp.GetRequiredService<IPlanReaderService>();
    var jobService = sp.GetRequiredService<IJobService>();
    var planWatcher = sp.GetRequiredService<IPlanWatcherService>();
    return new PlanCountsService(planReader, jobService, planWatcher);
});
server.Services.AddSingleton<IPlanCountsService>(sp => sp.GetRequiredService<PlanCountsService>());
server.Services.AddSingleton<InboxWatcherService>(sp =>
{
    var config = sp.GetRequiredService<IConfigService>();
    var jobService = sp.GetRequiredService<IJobService>();
    return new InboxWatcherService(config, jobService);
});
server.Services.AddSingleton<IInboxWatcherService>(sp => sp.GetRequiredService<InboxWatcherService>());
server.UseWebApplication(app =>
{
    // Publish the actual bound URL so child processes can reach this server,
    // even when --find-available-port shifts the port away from the default.
    var serverUrl = app.Urls.FirstOrDefault();
    if (serverUrl != null)
        Environment.SetEnvironmentVariable("TENDRIL_URL", serverUrl);

    // Eagerly resolve watcher services so their FileSystemWatchers start immediately
    app.Services.GetRequiredService<PlanWatcherService>();
    app.Services.GetRequiredService<IInboxWatcherService>();

    // Start database sync in background
    var syncService = app.Services.GetRequiredService<PlanDatabaseSyncService>();
    Task.Run(syncService.PerformInitialSync);
    app.Services.GetRequiredService<TelemetryService>().TrackAppStarted();
    app.UseAssets(server.Args, app.Services.GetRequiredService<ILogger<Server>>(), "Assets", "tendril/assets");
});
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
var version = typeof(TendrilAppShell).Assembly.GetName().Version!;
var versionString = version.ToString(3);
var appShellSettings = new AppShellSettings()
    .Header(
        Layout.Horizontal(
            new Image("/tendril/assets/Tendril.svg").Width(Size.Units(15)).Height(Size.Auto()),
            Layout.Vertical(
                Text.Block("Ivy Tendril"),
                Text.Muted($"v{versionString}")
            ).Gap(0)
        ).Gap(2).Padding(2).AlignContent(Align.BottomLeft)
    )
    .DefaultAppId("dashboard")
    .UseTabs(preventDuplicates: true);
server.UseAppShell(() => new TendrilAppShell(appShellSettings));
await server.RunAsync();
