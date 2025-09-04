using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.SquareChevronRight, path: ["Widgets"])]
public class PaginationApp() : SampleBase
{
    protected override object? BuildSample()
    {
        var label = this.UseState("Change the page");

        var eventHandler = (Event<Pagination, int> e) =>
        {
            label.Set($"Page was changed to {e.Value}.");
        };

        return Layout.Vertical()
               | Text.H1("Pagination")
               | Text.H2("Siblings")
               | new Pagination(10, 20, eventHandler).Siblings(0)
               | new Pagination(10, 20, eventHandler).Siblings(1)
               | new Pagination(10, 20, eventHandler).Siblings(2)
               | new Pagination(10, 20, eventHandler).Siblings(3)

               | Text.H2("Boundaries")
               | new Pagination(10, 20, eventHandler).Boundaries(0)
               | new Pagination(10, 20, eventHandler).Boundaries(1)
               | new Pagination(10, 20, eventHandler).Boundaries(2)
               | new Pagination(10, 20, eventHandler).Boundaries(3)

               | Text.H2("Interactive Demo")
               | Text.Literal(label.Value)
            ;
    }
}