using Ivy.Shared;

namespace Ivy.Samples.Apps.Other;

[App(icon: Icons.Palette)]
public class ColorsApp : ViewBase
{
    public override object? Build()
    {
        object GenerateColors()
        {
            Colors[] colors = Enum.GetValues<Colors>();
            var colorView = Layout.Vertical(
                colors.Select(color =>
                    new Box(color.ToString())
                        .Width(Size.Auto())
                        .Height(10)
                        .Color(color).BorderRadius(BorderRadius.Rounded)
                        .Padding(3)
                )
            );
            return colorView;
        }

        return Layout.Grid().Columns(2)
               | (Layout.Vertical().Padding(10) | GenerateColors())
               | (Layout.Vertical().Padding(10).Background(Colors.Black) | GenerateColors());
    }
}