namespace Ivy.Core.Apps;

public interface IAppRepository
{
    IObservable<Unit> Reloaded { get; }

    IObservable<IReadOnlySet<string>> AppsRefreshRequested { get; }

    MenuItem[] GetMenuItems();

    AppDescriptor GetAppOrDefault(string? id);

    AppDescriptor? GetApp(string id);

    AppDescriptor? GetApp(Type type);
}
