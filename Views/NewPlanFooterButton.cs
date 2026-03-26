using Ivy;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Views;

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
