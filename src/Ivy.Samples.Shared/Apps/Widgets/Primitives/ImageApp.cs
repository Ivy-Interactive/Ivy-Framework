
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Image, group: ["Widgets", "Primitives"], searchHints: ["picture", "photo", "img", "graphics", "media", "visual"])]
public class ImageApp : SampleBase
{
    protected override object? BuildSample()
    {
        var clickCount = this.UseState(0);

        return new object[]
        {
            new Image("https://placehold.co/600x400") { Alt = "A placeholder image", Caption = "A placeholder image" },
            new Image("https://placehold.co/600x400") { Caption = "Click to visit example.com" }.Link("https://example.com"),
            new Image("https://placehold.co/600x400") { Caption = $"Clicked {clickCount.Value} times" }.OnClick(() => clickCount.Value++),
            new Image("https://placehold.co/600x400") { Caption = "Solid border" }
                .BorderStyle(BorderStyle.Solid).BorderColor(Colors.Blue).BorderThickness(2).BorderRadius(BorderRadius.Rounded),
            new Image("https://placehold.co/600x400") { Caption = "Rounded border" }
                .BorderStyle(BorderStyle.Solid).BorderColor(Colors.Gray).BorderThickness(1).BorderRadius(BorderRadius.Full),
            new Image("https://placehold.co/600x400") { Caption = "Clickable with hover" }
                .BorderStyle(BorderStyle.Solid).BorderColor(Colors.Primary).BorderThickness(2).BorderRadius(BorderRadius.Rounded)
                .OnClick(() => clickCount.Value++),
            new Image("https://placehold.co/600x400") { ObjectFit = ImageFit.Cover, Caption = "ObjectFit: Cover" }.Width(Size.Units(50)).Height(Size.Units(50)),
            new Image("https://placehold.co/600x400") { ObjectFit = ImageFit.Contain, Caption = "ObjectFit: Contain" }.Width(Size.Units(50)).Height(Size.Units(50)),
            new Image("https://placehold.co/600x400") { ObjectFit = ImageFit.Fill, Caption = "ObjectFit: Fill" }.Width(Size.Units(50)).Height(Size.Units(50)),
            new Image("https://placehold.co/600x400") { ObjectFit = ImageFit.None, Caption = "ObjectFit: None" }.Width(Size.Units(50)).Height(Size.Units(50)),
            new Image("https://placehold.co/600x400") { ObjectFit = ImageFit.ScaleDown, Caption = "ObjectFit: ScaleDown" }.Width(Size.Units(50)).Height(Size.Units(50)),
        };
    }
}
