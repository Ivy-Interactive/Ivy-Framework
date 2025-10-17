using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.SquareChevronRight, path: ["Widgets"], searchHints: ["paging", "navigation", "pages", "next", "previous", "numbers"])]
public class PaginationApp() : SampleBase
{
    protected override object? BuildSample()
    {
        var page = this.UseState(10);

        var eventHandler = (Event<Pagination, int> e) =>
        {
            page.Set(e.Value);
        };

        return Layout.Vertical()
               | Text.H1("Pagination")
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