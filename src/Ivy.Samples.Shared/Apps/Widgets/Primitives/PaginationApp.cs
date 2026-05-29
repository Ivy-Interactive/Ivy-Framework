
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.SquareChevronRight, group: ["Widgets"], searchHints: ["paging", "navigation", "pages", "next", "previous", "numbers"])]
public class PaginationApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Overview", new PaginationOverviewSample()),
            new Tab("Density", new PaginationDensitySample())
        ).Variant(TabsVariant.Content);
    }
}

public class PaginationOverviewSample : ViewBase
{
    public override object? Build()
    {
        var page = UseState(10);

        var eventHandler = (Event<Pagination, int> e) =>
        {
            page.Set(e.Value);
        };

        return Layout.Vertical()
               | Text.H2("Siblings")
               | new Pagination(page.Value, 20, eventHandler).Siblings(0)
               | new Pagination(page.Value, 20, eventHandler).Siblings(1)
               | new Pagination(page.Value, 20, eventHandler).Siblings(2)
               | new Pagination(page.Value, 20, eventHandler).Siblings(3)

               | Text.H2("Boundaries")
               | new Pagination(page.Value, 20, eventHandler).Boundaries(0)
               | new Pagination(page.Value, 20, eventHandler).Boundaries(1)
               | new Pagination(page.Value, 20, eventHandler).Boundaries(2)
               | new Pagination(page.Value, 20, eventHandler).Boundaries(3);
    }
}

public class PaginationDensitySample : ViewBase
{
    public override object? Build()
    {
        var page = UseState(5);
        var density = UseState(() => Density.Medium);

        var onChange = (Event<Pagination, int> e) =>
        {
            page.Set(e.Value);
        };

        return Layout.Vertical().Gap(4)
               | Layout.Horizontal().Gap(2)
                   | new Button("Small").OnClick(_ => density.Set(Density.Small))
                       .Variant(density.Value == Density.Small ? ButtonVariant.Primary : ButtonVariant.Outline).Small()
                   | new Button("Medium").OnClick(_ => density.Set(Density.Medium))
                       .Variant(density.Value == Density.Medium ? ButtonVariant.Primary : ButtonVariant.Outline).Small()
                   | new Button("Large").OnClick(_ => density.Set(Density.Large))
                       .Variant(density.Value == Density.Large ? ButtonVariant.Primary : ButtonVariant.Outline).Small()
               | new Pagination(page.Value, 20, onChange).Siblings(1).Density(density.Value)
               | new Pagination(page.Value, 20, onChange).Siblings(2).Density(density.Value)
               | new Pagination(page.Value, 20, onChange).Siblings(3).Density(density.Value);
    }
}
