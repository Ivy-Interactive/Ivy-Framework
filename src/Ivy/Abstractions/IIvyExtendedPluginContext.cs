using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Plugins;

public interface IIvyExtendedPluginContext : IIvyPluginContext
{
    // App registration
    void AddApp(AppDescriptor descriptor);
    void AddAppsFromAssembly(Assembly assembly);

    // ASP.NET pipeline
    void UseWebApplication(Action<WebApplication> configure);
    void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure);
}
