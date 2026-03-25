using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril;
using Ivy.Tendril.Apps;
using Ivy.Tendril.Services;

var server = new Server();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.Services.AddSingleton<ConfigService>();
server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<PlanReaderService>();
server.Services.AddSingleton<TaskService>();
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseAppShell(new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .Footer(new NewPlanFooterButton()));
await server.RunAsync();

public class NewPlanFooterButton : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        return new Button("New Plan")
            .Icon(Icons.Plus)
            .Width(Size.Full())
            .Variant(ButtonVariant.Outline)
            .OnClick(() => navigator.Navigate<PlansApp>());
    }
}
