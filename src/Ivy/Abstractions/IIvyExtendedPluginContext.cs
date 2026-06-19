using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Plugins;

public interface IIvyExtendedPluginContext : IIvyPluginContext
{
    // App registration
    void AddApp(AppDescriptor descriptor);
    void AddAppsFromAssembly(Assembly assembly);

    // Menu hooks
    void AddMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer, int priority = 0);

    // Sidebar badge providers
    void AddBadgeProvider(string menuTag, Func<IServiceProvider, int> countProvider);

    // ASP.NET pipeline
    void UseWebApplication(Action<WebApplication> configure);
    void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure);
}
