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

        var tabsLayout = new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
            tabs.Value.ToArray()
        ).Variant(TabsVariant.Tabs).Width(150);

        var addBtn = new Button("Add Tab").HandleClick(() =>
        {
            tabs.Set(tabs.Value.Add(new Tab($"Tab {tabs.Value.Length + 1}", $"Tab {tabs.Value.Length + 1}")));
        });

        return new Fragment()
               | new FloatingPanel(addBtn, Align.BottomLeft)
               | tabsLayout
            ;

    }
}