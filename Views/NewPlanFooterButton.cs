using Ivy;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Views;

public class NewPlanFooterButton : ViewBase
{
    public override object? Build()
    {
        var jobService = UseService<JobService>();
        var configService = UseService<ConfigService>();
        var dialogOpen = UseState(false);
        var lastSelectedProject = UseState("[Auto]");

        var projectNames = configService.Projects.Select(p => p.Name).ToList();

        var elements = new List<object>
        {
            new Button("New Idea")
                .Icon(Icons.Plus)
                .Width(Size.Full())
                .Variant(ButtonVariant.Outline)
                .OnClick(() => dialogOpen.Set(true))
                .ShortcutKey("CTRL+ALT+1")
        };

        if (dialogOpen.Value)
        {
            elements.Add(new CreatePlanDialog(
                projectNames: projectNames,
                onCreatePlan: (description, project) =>
                {
                    lastSelectedProject.Set(project);
                    jobService.StartJob("MakePlan", "-Description", description, "-Project", project);
                },
                onClose: () => dialogOpen.Set(false),
                defaultProject: lastSelectedProject.Value
            ));
        }

        return new Fragment(elements.ToArray());
    }
}
