using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Grid2x2, searchHints: ["toolbar", "sticky", "fixed", "navigation", "header", "scroll"])]
public class HeaderLayoutApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new HeaderLayoutView();
    }
}

public class HeaderLayoutView : ViewBase
{
    public override object? Build()
    {
        var counter = UseState(0);
        var searchText = UseState("");
        var client = UseService<IClientProvider>();

        void OnAddItem(Event<Button> @event)
        {
            counter.Set(counter.Value + 1);
            client.Toast($"Added item {counter.Value + 1}");
        }

        void OnClearItems(Event<Button> @event)
        {
            counter.Set(0);
            searchText.Set("");
            client.Toast("Cleared all items");
        }

        void OnExport(Event<Button> @event)
        {
            client.Toast("Exporting data...");
        }

        // Create the fixed header/toolbar
        var toolbar = new Card(
            Layout.Horizontal().Gap(4)
                | searchText.ToTextInput()
                    .Placeholder("Search items...")
                    .Variant(TextInputs.Search)
                | new Button("Add Item")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(OnAddItem)
                | new Button("Clear All")
                    .Icon(Icons.Trash)
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(OnClearItems)
                | new Button("Export")
                    .Icon(Icons.Download)
                    .Variant(ButtonVariant.Ghost)
                    .HandleClick(OnExport)
        );

        // Create scrollable content with many items
        var contentItems = new List<object>
        {
            new Card("Welcome to HeaderLayout Demo")
                .Title("Getting Started")
                .Description("This toolbar above stays fixed while you scroll through the content below."),

            new Card("Fixed Header Behavior")
                .Title("Key Feature")
                .Description("The header remains visible at all times, making actions always accessible."),
        };

        // Add dynamic items based on counter
        for (int i = 1; i <= Math.Max(20, counter.Value); i++)
        {
            contentItems.Add(
                new Card($"Content Item {i}")
                    .Title($"Item #{i}")
                    .Description($"This is sample content item number {i}. Created at {DateTime.Now.AddMinutes(-i):HH:mm}. The toolbar above remains fixed while you scroll through all these items.")
            );
        }

        // Add some informational cards at the end
        contentItems.AddRange(new[]
        {
            new Card("Scroll Behavior")
                .Title("Try Scrolling")
                .Description("Notice how the toolbar stays at the top while this content scrolls. This is perfect for data tables, lists, and dashboards."),

            new Card("Interactive Actions")
                .Title("Test the Buttons")
                .Description("Try clicking 'Add Item' to see more content appear, or 'Clear All' to reset everything."),

            new Card("Real-world Usage")
                .Title("Common Patterns")
                .Description("HeaderLayout is commonly used for admin panels, data management interfaces, content browsers, and any interface where you need persistent controls.")
        });

        var content = Layout.Vertical().Gap(4) | contentItems.ToArray();

        // Use HeaderLayout with constrained height to demonstrate scrolling
        return new HeaderLayout(toolbar, content).Height(Size.Units(500));
    }
}
