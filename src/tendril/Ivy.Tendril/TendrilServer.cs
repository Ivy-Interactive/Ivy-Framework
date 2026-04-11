using System.ClientModel;
using Ivy.Tendril.AppShell;
using Ivy.Tendril.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Ivy.Tendril;

public static class TendrilServer
{
    public static Server Create(string[] args)
    {
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
        server.Services.AddSingleton(modelPricingService);

        // Register IChatClient if LLM is configured
        if (configService.Settings.Llm is { } llmConfig && !string.IsNullOrEmpty(llmConfig.ApiKey))
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

        server.Services.AddSingleton<OnboardingSetupService>();
        server.Services.AddSingleton<IOnboardingSetupService>(sp => sp.GetRequiredService<OnboardingSetupService>());
        server.Services.AddSingleton<GithubService>();
        server.Services.AddSingleton<IGithubService>(sp => sp.GetRequiredService<GithubService>());
        server.Services.AddSingleton<GitService>();
        server.Services.AddSingleton<IGitService>(sp => sp.GetRequiredService<GitService>());
        server.Services.AddSingleton<PlanReaderService>(sp =>
        {
            var planService = new PlanReaderService(
                sp.GetRequiredService<IConfigService>(),
                sp.GetRequiredService<ILogger<PlanReaderService>>(),
                sp.GetRequiredService<ITelemetryService>());
            planService.RepairPlans();
            planService.RecoverStuckPlans();
            return planService;
        });
        server.Services.AddSingleton<IPlanReaderService>(sp => sp.GetRequiredService<PlanReaderService>());

        // SQLite database service
        server.Services.AddSingleton<IPlanDatabaseService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfigService>();
            if (string.IsNullOrEmpty(cfg.TendrilHome))
                throw new InvalidOperationException("Cannot create PlanDatabaseService: TendrilHome is not configured. Complete onboarding first.");
            var dbPath = Path.Combine(cfg.TendrilHome, "tendril.db");
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<PlanDatabaseService>();
            return new PlanDatabaseService(dbPath, logger);
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
            var logger = sp.GetRequiredService<ILogger<TelemetryService>>();
            return new TelemetryService(config.Settings.Telemetry, logger);
        });
        server.Services.AddSingleton<TelemetryService>(sp =>
            (TelemetryService)sp.GetRequiredService<ITelemetryService>());
        server.Services.AddSingleton<JobService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfigService>();
            return new JobService(
                cfg,
                sp.GetRequiredService<ModelPricingService>(),
                sp.GetRequiredService<IPlanReaderService>(),
                sp.GetRequiredService<ITelemetryService>(),
                sp.GetRequiredService<IPlanWatcherService>(),
                string.IsNullOrEmpty(cfg.TendrilHome) ? null : sp.GetRequiredService<IPlanDatabaseService>());
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
        server.Services.AddSingleton<WorktreeCleanupService>(sp =>
        {
            var config = sp.GetRequiredService<IConfigService>();
            var logger = sp.GetRequiredService<ILogger<WorktreeCleanupService>>();
            return new WorktreeCleanupService(config.PlanFolder, logger);
        });
        server.Services.AddSingleton<IStartable>(sp => sp.GetRequiredService<WorktreeCleanupService>());
        server.Services.AddSingleton<PrStatusSyncService>(sp =>
        {
            var database = sp.GetRequiredService<IPlanDatabaseService>();
            var githubService = sp.GetRequiredService<IGithubService>();
            var planReader = sp.GetRequiredService<IPlanReaderService>();
            var logger = sp.GetRequiredService<ILogger<PrStatusSyncService>>();
            return new PrStatusSyncService(database, githubService, planReader, logger);
        });
        server.Services.AddSingleton<IStartable>(sp => sp.GetRequiredService<PrStatusSyncService>());

        server.UseWebApplication(app =>
        {
            // Publish the actual bound URL so child processes can reach this server
            var serverUrl = app.Urls.FirstOrDefault();
            if (serverUrl != null)
                Environment.SetEnvironmentVariable("TENDRIL_URL", serverUrl);

            if (!configService.NeedsOnboarding)
                BackgroundServiceActivator.Start(app.Services);

            var telemetryService = app.Services.GetRequiredService<TelemetryService>();
            var appVersion = typeof(TendrilAppShell).Assembly.GetName().Version!.ToString(3);
            telemetryService.TrackAppStarted(new AppStartContext(
                appVersion,
                configService.Settings.Projects.Count,
                configService.Settings.Llm?.ApiKey != null));
            _ = Task.Run(async () =>
            {
                try
                {
                    await telemetryService.IdentifyAsync(appVersion);
                    await telemetryService.FlushAsync();
                }
                catch (Exception ex)
                {
                    Program.WriteCrashLog($"[{DateTime.UtcNow:O}] Telemetry startup exception: {ex}");
                }
            });
            app.UseAssets(server.Args, app.Services.GetRequiredService<ILogger<Server>>(), "Assets", "tendril/assets");
        });

        server.AddAppsFromAssembly(typeof(TendrilServer).Assembly);
        server.AddConnectionsFromAssembly(typeof(TendrilServer).Assembly);

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
            .WallpaperApp<Apps.WallpaperApp>()
            .UseTabs(true);

        server.UseAppShell(() => new TendrilAppShell(appShellSettings));

        return server;
    }
}