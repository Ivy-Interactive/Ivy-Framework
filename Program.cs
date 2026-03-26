using Ivy;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Tendril;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

// Set QuestPDF license for non-production/community usage
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
        var taskService = UseService<TaskService>();
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
                    taskService.StartTask("MakePlan", description);
                },
                onClose: () => dialogOpen.Set(false)
            ));
        }

        return new Fragment(elements.ToArray());
    }
}
