using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutTemplate, group: ["Widgets", "Layouts"], searchHints: ["navigation", "panels", "pages", "switcher", "tabbed", "sections"])]
public class TabsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedIndex = UseState<int?>();
        var tabs = UseState(() => ImmutableArray.Create<Tab>([
            new Tab("Customers", "Customers").Icon(Icons.User).Badge("10"),
            new Tab("Orders", "Orders").Icon(Icons.DollarSign).Badge("0"),
            new Tab("Settings", "Settings").Icon(Icons.Settings).Badge("999")
        ]));

        void OnTabClose(int index)
        {
            //[0,1,|2|,3] -> 2
            //[0,1,|2|] -> 1
            //[0,|1|] -> 0
            //[|0|] -> null
            var newIndex = Math.Min(index, tabs.Value.Length - 2);
            selectedIndex.Set(newIndex >= 0 ? newIndex : null);
            tabs.Set(tabs.Value.RemoveAt(index));
        }

        void OnTabCloseOthers(int index)
        {
            var keptTab = tabs.Value[index];
            tabs.Set(ImmutableArray.Create(keptTab));
            selectedIndex.Set(0);
        }

        void OnAddButtonClick()
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
            | Layout.Tabs(tabs.Value.ToArray())
                .Variant(TabsVariant.Tabs)
                .OnSelect(i => selectedIndex.Set(i))
                .OnClose(OnTabClose)
                .OnCloseOthers(OnTabCloseOthers)
                .AddButton("+", OnAddButtonClick)
                .SelectedIndex(selectedIndex.Value)
            | Text.H3("Content variant")
            | Layout.Tabs(tabs.Value.ToArray())
                .Variant(TabsVariant.Content)
                .OnSelect(i => selectedIndex.Set(i))
                .OnClose(OnTabClose)
                .OnCloseOthers(OnTabCloseOthers)
                .AddButton("+", OnAddButtonClick)
                .SelectedIndex(selectedIndex.Value);
    }
}
