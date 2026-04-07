using System.Collections.Immutable;
using Ivy.Core;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutTemplate, group: ["Widgets", "Layouts"], searchHints: ["navigation", "panels", "pages", "switcher", "tabbed", "sections"])]
public class TabsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedIndex = UseState<int?>();
        var client = UseService<IClientProvider>();
        var tabs = UseState(() => ImmutableArray.Create<Tab>([
            new Tab("Customers", "Customers").Icon(Icons.User).Badge("10"),
            new Tab("Orders", "Orders").Icon(Icons.DollarSign).Badge("0"),
            new Tab("Settings", "Settings").Icon(Icons.Settings).Badge("999")
        ]));
        var width = UseState(1.0);

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

        void OnTabCloseOthers(Event<TabsLayout, int> @event)
        {
            var keptTab = tabs.Value[@event.Value];
            tabs.Set(ImmutableArray.Create(keptTab));
            selectedIndex.Set(0);
        }

        void OnAddButtonClick(Event<TabsLayout, int> @event)
        {
            tabs.Set(tabs.Value.Add(new Tab($"Tab {tabs.Value.Length + 1}", $"Tab {tabs.Value.Length + 1}")));
        }

        return Layout.Vertical()
            | Text.H1("Tabs layout")
            | Text.P("Use Layout.Tabs() to create tabbed interfaces.")
            | Text.H2("Variants")
            | Text.H3("Content variant (default)")
            | Layout.Tabs(
                new Tab("Customers", "Customers").Icon(Icons.User).Badge("10"),
                new Tab("Orders", "Orders").Icon(Icons.DollarSign).Badge("0"),
                new Tab("Settings", "Settings").Icon(Icons.Settings).Badge("999")
            ).Variant(TabsVariant.Content)
            | Text.H3("Tabs variant")
            | (new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
                tabs.Value.ToArray()
            ).Variant(TabsVariant.Tabs).Width(Size.Fraction((float)width.Value)).AddButton("+", OnAddButtonClick)
                with
            { OnCloseOthers = ((Action<Event<TabsLayout, int>>)OnTabCloseOthers).ToEventHandler() })
            | Text.H3("Content variant")
            | (new TabsLayout(OnTabSelect, OnTabClose, null, null, selectedIndex.Value,
                tabs.Value.ToArray()
            ).Variant(TabsVariant.Content).Width(Size.Fraction((float)width.Value)).AddButton("+", OnAddButtonClick)
                with
            { OnCloseOthers = ((Action<Event<TabsLayout, int>>)OnTabCloseOthers).ToEventHandler() });
    }
}
