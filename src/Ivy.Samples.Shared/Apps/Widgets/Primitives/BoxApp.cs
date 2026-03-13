
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Pencil, searchHints: ["container", "div", "wrapper", "rectangle", "styling"])]
public class BoxApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        //Get all colors:
        Colors[] colors = (Colors[])Enum.GetValues(typeof(Colors));

        var colorView = Layout.Wrap(
            colors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(Size.Units(10))
                    .Background(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );

        var box = new Box().Height(Size.Units(10)).Width(Size.Fit()).Padding(2);

        return Layout.Vertical()
               | Text.H1("Box Widget")
               | Text.H2("Colors")
               | colorView

               | Text.H2("Width and Height")
               | new DemoView(_ => new Box()
                   .Height(Size.Units(20))
                   .Width(Size.Units(20)))

               | Text.H2("Border Style")
               | new DemoView(_ => box.BorderStyle(BorderStyle.Dotted).Content("BorderStyle.Dotted"))
               | (Layout.Horizontal()
                  | box.BorderStyle(BorderStyle.Dashed).Content("BorderStyle.Dashed")
                  | box.BorderStyle(BorderStyle.Solid).Content("BorderStyle.Solid")
                  | box.BorderStyle(BorderStyle.None).Content("BorderStyle.None")
               )

               | Text.H2("Border Thickness")
               | new DemoView(_ => box.BorderThickness(1).Content("1"))
               | (Layout.Horizontal()
                  | box.BorderThickness(0).Content("0")
                  | box.BorderThickness(1).Content("1")
                  | box.BorderThickness(2).Content("2")
                  | box.BorderThickness(3).Content("3")
                  | box.BorderThickness(4).Content("4")
               )

               | Text.H2("Border Radius")
               | new DemoView(_ => box.BorderRadius(BorderRadius.Rounded).Content("BorderRadius.Rounded"))
               | (Layout.Horizontal()
                  | box.BorderRadius(BorderRadius.Full).Content("BorderRadius.Full")
                  | box.BorderRadius(BorderRadius.None).Content("BorderRadius.None")
               )

               | Text.H2("OnClick")
               | new DemoView(_ => box.OnClick(() => client.Toast("Box clicked!")).Content("Clickable Box"))
               | (Layout.Horizontal()
                  | box.OnClick(() => client.Toast("Click me clicked!")).Content("Click me!")
               )

               | Text.H2("Hover Variants")
               | new DemoView(_ => box.Hover(CardHoverVariant.PointerAndTranslate).Content("CardHoverVariant.PointerAndTranslate"))
               | (Layout.Horizontal()
                  | box.Hover(CardHoverVariant.None).Content("None")
                  | box.Hover(CardHoverVariant.Pointer).Content("Pointer")
                  | box.Hover(CardHoverVariant.PointerAndTranslate).Content("PointerAndTranslate")
               )

            ;
    }
}
