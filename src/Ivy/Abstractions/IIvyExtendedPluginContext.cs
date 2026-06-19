using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Plugins;

public interface IIvyExtendedPluginContext : IIvyPluginContext
{
    // App registration
    void AddApp(AppDescriptor descriptor);
    void AddAppsFromAssembly(Assembly assembly);

    // Menu hooks
    void TransformMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer, int priority = 0);

    // ASP.NET pipeline
    void UseWebApplication(Action<WebApplication> configure);
    void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure);
}
