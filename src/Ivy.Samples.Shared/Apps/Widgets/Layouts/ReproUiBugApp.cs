using Ivy;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

public record Plan(int Id, string Title, string Queue, string Level);

[App(icon: Icons.Bug, isVisible: false)]
public class ReproUiBugApp : ViewBase
{
    private List<Plan> _allPlans = [
        new Plan(1, "Premium Plan", "Standard", "Level 1"),
        new Plan(2, "Enterprise Plan", "Priority", "Level 5"),
        new Plan(3, "Free Plan", "Low", "Level 1")
    ];

    private Plan _selectedPlan => _allPlans[0];
    private int currentIndex = 0;

    public override object? Build()
    {
        var isEditing = UseState(false);

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Queue).Variant(BadgeVariant.Info)
            | new Badge(_selectedPlan.Level).Variant(BadgeVariant.Warning)
            | isEditing.ToSwitchInput(Icons.Pencil).Label("Edit")
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true)
            ;

        return Layout.Vertical().Padding(5)
            | Text.H1("Reproduction of UI Bug")
            | header
            | (isEditing.Value ? "Editing mode is ON" : "Editing mode is OFF")
            ;
    }
}
