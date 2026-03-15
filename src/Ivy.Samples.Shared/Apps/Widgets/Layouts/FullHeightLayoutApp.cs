namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.RectangleVertical, searchHints: ["full height", "header", "footer", "content", "stretch", "fill"])]
public class FullHeightLayoutApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new FullHeightLayoutView();
    }
}

/// <summary>
/// Demonstrates the correct way to build a header + content + footer layout
/// using Layout.Vertical().Height(Size.Full()). Only the content row should
/// have .Height(Size.Full()) — header and footer rows auto-size.
/// </summary>
public class FullHeightLayoutView : ViewBase
{
    public override object? Build()
    {
        var filter = UseState("");

        var header = Layout.Horizontal().Align(Align.Center)
            | Text.H2("Items")
            | filter.ToTextInput().Placeholder("Filter...").Variant(TextInputVariant.Search);

        var content = Layout.Vertical()
            | Enumerable.Range(1, 30)
                .Select(i => new Card($"Item {i}").Description($"Description for item {i}"))
                .Cast<object>()
                .ToArray();

        var footer = Layout.Horizontal().Align(Align.Center)
            | Text.Muted("30 items")
            | new Spacer()
            | new Button("Export").Variant(ButtonVariant.Outline).Icon(Icons.Download);

        return Layout.Vertical().Height(Size.Full())
            | header
            | (Layout.Vertical().Height(Size.Full())
                | content)
            | footer;
    }
}
