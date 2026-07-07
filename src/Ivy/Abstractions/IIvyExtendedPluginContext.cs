using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Ivy.Plugins;

public interface IIvyExtendedPluginContext : IIvyPluginContext
{
    // App registration
    void AddApp(AppDescriptor descriptor);
    void AddAppsFromAssembly(Assembly assembly);

    // HTTP endpoints
    void UseEndpoints(string slug, Action<IEndpointRouteBuilder> configure);

    // ASP.NET pipeline
    void UseWebApplication(Action<WebApplication> configure);
    void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure);
}
