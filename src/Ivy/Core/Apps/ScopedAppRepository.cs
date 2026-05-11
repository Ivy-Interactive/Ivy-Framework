namespace Ivy.Core.Apps;

internal class ScopedAppRepository(IAppRepository inner, string targetId, AppDescriptor? overrideApp) : IAppRepository
{
    public IObservable<Unit> Reloaded => inner.Reloaded;
    public IObservable<IReadOnlySet<string>> AppsRefreshRequested => inner.AppsRefreshRequested;

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