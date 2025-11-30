using Ivy;
using Ivy.Shared;

namespace Ivy.Docs.Tools;

public static class BoxExtensions
{
    public static Box Plain(this Box box) => box with
    {
        BorderThickness = new(1),
        Padding = new(4),
        BorderRadius = BorderRadius.Rounded,
        BorderStyle = BorderStyle.Solid,
        ContentAlign = Align.TopLeft,
        Color = null
    };
}
