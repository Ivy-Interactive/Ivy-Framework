using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutTemplate, path: ["Widgets", "Layouts"])]
public class TabsApp : ViewBase
{
    public override object? Build()
    {
        var selectedIndex = this.UseState<int?>();
        var client = UseService<IClientProvider>();
        var tabs = UseState(() => ImmutableArray.Create<Tab>([
            new Tab("Customers", "Customers").Icon(Icons.User).Badge("10"),
            new Tab("Orders", "Orders").Icon(Icons.DollarSign).Badge("0"),
            new Tab("Settings", "Settings").Icon(Icons.Settings).Badge("999")
        ]));

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            selectedIndex.Set(@event.Value);
        }

        void OnTabClose(Event<TabsLayout, int> @event)
        {
            //[0,1,|2|,3] -> 2
            //[0,1,|2|] -> 1
            //[0,|1|] -> 0
            //[|0|] -> null
            var newIndex = Math.Min(@event.Value, tabs.Value.Length - 2);
            selectedIndex.Set(newIndex >= 0 ? newIndex : null);
            tabs.Set(tabs.Value.RemoveAt(@event.Value));
        }

        var addBtn = new Button("Add Tab").HandleClick(() =>
        {
            tabs.Set(tabs.Value.Add(new Tab($"Tab {tabs.Value.Length + 1}", $"Tab {tabs.Value.Length + 1}")));
        });

        var width = this.UseState(1.0);

        return Layout.Vertical()
            | Text.H1("Tabs layout")
            | Text.P("Adjust the width to see how the tabs react on mobile.")
            | width.ToSliderInput().Min(0f).Max(1f).WithLabel("Width")
            | Text.H2("Variants")
            | new FloatingPanel(addBtn, Align.BottomLeft)
            | Text.H3("Tabs variant")
            | new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
                tabs.Value.ToArray()
            ).Variant(TabsVariant.Tabs).Width(width.Value)
            | Text.H3("Content variant")
            | new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
                tabs.Value.ToArray()
            ).Variant(TabsVariant.Content).Width(width.Value);
    }
}