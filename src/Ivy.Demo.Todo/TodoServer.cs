namespace Ivy.Demo.Todo;

public static class TodoServer
{
    public static async Task RunAsync()
    {
        var server = new Server();
        server.UseHotReload();
        server.AddAppsFromAssembly(typeof(TodoServer).Assembly);
        server.SetMetaTitle("Todo Demo");
        server.UseAppShell(() => new DefaultSidebarAppShell(new AppShellSettings()));
        await server.RunAsync();
    }
}
