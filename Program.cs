using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

var server = new Server();
server.DangerouslyAllowLocalFiles();
server.UseCulture("en-US");
#if DEBUG
server.UseHotReload();
#endif
server.SetMetaTitle("Ivy Tendril");
server.Services.AddSingleton<ConfigService>();
server.Services.AddSingleton<GithubService>();
server.Services.AddSingleton<PlanReaderService>();
server.Services.AddSingleton<JobService>();
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
        var jobService = UseService<JobService>();
        var dialogOpen = UseState(false);

        var elements = new List<object>
        {
            new Button("Make Plan")
                .Icon(Icons.Plus)
                .Width(Size.Full())
                .Variant(ButtonVariant.Outline)
                .OnClick(() => dialogOpen.Set(true))
                .ShortcutKey("CTRL+ALT+M")
        };

        if (dialogOpen.Value)
        {
            elements.Add(new CreatePlanDialog(
                onCreatePlan: description =>
                {
                    jobService.StartJob("MakePlan", description);
                },
                onClose: () => dialogOpen.Set(false)
            ));
        }

        return new Fragment(elements.ToArray());
    }
}
