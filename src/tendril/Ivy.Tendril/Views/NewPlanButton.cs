using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Views;

public class NewPlanButton : ViewBase
{
    public override object Build()
    {
        var jobService = UseService<IJobService>();
        var configService = UseService<IConfigService>();
        var dialogOpen = UseState(false);
        var lastSelectedProjects = UseState<string[]>(["[Auto]"]);

        var projectNames = configService.Projects.Select(p => p.Name).ToList();

        var elements = new List<object>
        {
            new Button("New Plan")
                .Icon(Icons.Plus)
                .Width(Size.Full())
                .Variant(ButtonVariant.Primary)
                .OnClick(() => dialogOpen.Set(true))
                .ShortcutKey("CTRL+ALT+N")
        };

        if (dialogOpen.Value)
            elements.Add(new CreatePlanDialog(
                projectNames,
                (description, projects, priority) =>
                {
                    lastSelectedProjects.Set(projects);
                    var project = string.Join(",", projects);
                    jobService.StartJob("MakePlan", "-Description", $"{description} [FORCE]", "-Project", project, "-Priority", priority.ToString());
                },
                () => dialogOpen.Set(false),
                lastSelectedProjects.Value
            ));

        return new Fragment(elements.ToArray());
    }
}
