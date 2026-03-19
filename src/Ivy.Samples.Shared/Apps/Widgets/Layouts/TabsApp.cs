
namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutTemplate, path: ["Widgets", "Layouts"], searchHints: ["navigation", "panels", "pages", "switcher", "tabbed", "sections"])]
public class TabsApp : SampleBase
{
    protected override object? BuildSample()
    {
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
            | Layout.Tabs(
                new Tab("Customers", "Customers").Icon(Icons.User).Badge("10"),
                new Tab("Orders", "Orders").Icon(Icons.DollarSign).Badge("0"),
                new Tab("Settings", "Settings").Icon(Icons.Settings).Badge("999")
            ).Variant(TabsVariant.Tabs);
    }
}
