namespace Ivy.Samples.Shared.Apps.Tests;

public record Plan(int Id, string Title, string Queue, string Level);

[App(id: "header-alignment-test", title: "Header Alignment Test", icon: Icons.LayoutPanelTop, isVisible: false)]
public class HeaderAlignmentTestApp : SampleBase
{
    private List<Plan> _allPlans = [
        new Plan(1, "Premium Plan", "Standard", "Level 1"),
        new Plan(2, "Enterprise Plan", "Priority", "Level 5"),
        new Plan(3, "Free Plan", "Low", "Level 1")
    ];

    private Plan _selectedPlan => _allPlans[0];
    private int currentIndex = 0;

    protected override object? BuildSample()
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
            | Text.H1("Header Alignment Test")
            | header
            | (isEditing.Value ? "Editing mode is ON" : "Editing mode is OFF")
            ;
    }
}
