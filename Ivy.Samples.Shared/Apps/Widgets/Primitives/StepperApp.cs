using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.ListOrdered, path: ["Widgets", "Primitives"], searchHints: ["stepper", "steps", "wizard", "progress", "sequence"])]
public class StepperApp : SampleBase
{
    StepperItem[] GetItems(int selectedIndex) =>
    [
        new("1", selectedIndex>0 ? Icons.Check : null, "Company", "Some description"),
        new("2", selectedIndex>1 ? Icons.Check : null, "Raise", "Some description"),
        new("3", selectedIndex>2 ? Icons.Check : null, "Deck", "Some description"),
        new("4", null, "Founders", "Some description"),
    ];

    protected override object? BuildSample()
    {
        var selectedIndex = UseState(0);

        var items = GetItems(selectedIndex.Value);

        return Layout.Vertical()
               | new Stepper(OnSelect, selectedIndex.Value, items).Width(200)
               | Text.H3("With AllowSelectForward")
               | new Stepper(OnSelect, selectedIndex.Value, items).Width(200).AllowSelectForward()
               | (Layout.Horizontal().Gap(0)
                  | new Button("Previous").Link().HandleClick(() =>
                  {
                      selectedIndex.Set(Math.Clamp(selectedIndex.Value - 1, 0, items.Length - 1));
                  })
                  | new Button("Next").Link().HandleClick(() =>
                  {
                      selectedIndex.Set(Math.Clamp(selectedIndex.Value + 1, 0, items.Length - 1));
                  })
               )
            ;

        ValueTask OnSelect(Event<Stepper, int> e)
        {
            selectedIndex.Set(e.Value);
            return ValueTask.CompletedTask;
        }
    }
}
