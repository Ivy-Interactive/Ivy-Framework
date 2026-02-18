
namespace Ivy.Core.Apps;

public interface IAppRepository
{
    MenuItem[] GetMenuItems();

    AppDescriptor GetAppOrDefault(string? id);

    AppDescriptor? GetApp(string id);

    AppDescriptor? GetApp(Type type);
}
