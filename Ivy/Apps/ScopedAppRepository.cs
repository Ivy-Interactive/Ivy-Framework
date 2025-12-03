using Ivy.Shared;

namespace Ivy.Apps;

internal class ScopedAppRepository(IAppRepository inner, string targetId, AppDescriptor? overrideApp) : IAppRepository
{
    public MenuItem[] GetMenuItems() => inner.GetMenuItems();

    public AppDescriptor GetAppOrDefault(string? id)
    {
        if (overrideApp != null && id == targetId)
        {
            return overrideApp;
        }
        return inner.GetAppOrDefault(id);
    }

    public AppDescriptor? GetApp(string id)
    {
        if (overrideApp != null && id == targetId)
        {
            return overrideApp;
        }
        return inner.GetApp(id);
    }

    public AppDescriptor? GetApp(Type type) => inner.GetApp(type);
}