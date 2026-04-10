using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Tendril.Services;

public static class BackgroundServiceActivator
{
    public static void Start(IServiceProvider services)
    {
        services.GetRequiredService<IPlanWatcherService>();
        services.GetRequiredService<IInboxWatcherService>();
        foreach (var startable in services.GetServices<IStartable>())
            startable.Start();

        var syncService = services.GetRequiredService<PlanDatabaseSyncService>();
        _ = Task.Run(syncService.PerformInitialSync);
    }
}
