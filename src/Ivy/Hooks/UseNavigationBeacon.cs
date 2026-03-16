using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseNavigationBeaconExtensions
{
    /// <summary>
    /// Returns a beacon for entity type T if one is registered, enabling type-safe navigation.
    /// </summary>
    public static NavigationBeacon<T>? UseNavigationBeacon<T>(this IViewContext context)
    {
        var registry = context.UseService<INavigationBeaconRegistry>();
        return registry.GetBeacon<T>();
    }
}
