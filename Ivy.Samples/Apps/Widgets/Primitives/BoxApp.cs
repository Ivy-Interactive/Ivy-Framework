using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Pencil)]
public class BoxApp : SampleBase
{
    protected override object? BuildSample()
    {
        //Get all colors:
        Colors[] colors = (Colors[])Enum.GetValues(typeof(Colors));

        var colorView = Layout.Wrap(
            colors.Select(color =>
                new Box(color.ToString())
                    .Width(Size.Auto())
                    .Height(10)
                    .Color(color).BorderRadius(BorderRadius.Rounded)
                    .Padding(3)
            )
        );

        var box = new Box().Height(10).Width(Size.Fit()).Padding(2);

        return Layout.Vertical()
               | Text.H1("Box Widget")
               | Text.H2("Colors")
               | colorView

               | Text.H2("Width and Height")
               | new DemoView(_ => new Box()
                   .Height(20)
                   .Width(20))

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

            ;
    }
}