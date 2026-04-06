namespace Ivy.Core.Apps;

public interface IAppRepository
{
    IObservable<Unit> Reloaded { get; }

    MenuItem[] GetMenuItems();

    AppDescriptor GetAppOrDefault(string? id);

    AppDescriptor? GetApp(string id);

    AppDescriptor? GetApp(Type type);
}
