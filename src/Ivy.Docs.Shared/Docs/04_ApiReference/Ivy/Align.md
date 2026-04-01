---
searchHints:
  - alignment
  - positioning
  - center
  - justify
  - layout
  - vertical
---

# Align

`Align` specifies alignment options for UI elements across the Ivy framework. It is commonly used for aligning content within [layouts](../../../01_Onboarding/02_Concepts/04_Layout.md) such as `Layout.Horizontal` or `Layout.Vertical`, and in [widgets](../../../01_Onboarding/02_Concepts/03_Widgets.md) like [Box](../../../02_Widgets/01_Primitives/04_Box.md) via `ContentAlign`.

```csharp demo-tabs 
public class AlignView : ViewBase
{
    public override object? Build()
    {
        var squareBox = new Box().Width(Size.Units(5)).Height(Size.Units(5));
        var tallBox = new Box().Width(Size.Units(5)).Height(Size.Units(7));
        var tallestBox = new Box().Width(Size.Units(5)).Height(Size.Units(10));
        var wideBox = new Box().Width(Size.Units(7)).Height(Size.Units(5));
        var widestBox = new Box().Width(Size.Units(10)).Height(Size.Units(5));

        var container = new Box().Width(Size.Units(32)).Height(Size.Units(32)).Background(Colors.Pink).Padding(0).ContentAlign(null);

        object AlignHorizontalTest(Align align) =>
            container.Content(
                Layout.Horizontal().AlignContent(align) | squareBox | tallBox | tallestBox
            );

        object AlignVerticalTest(Align align) =>
            container.Content(
                Layout.Vertical().AlignContent(align).Height(Size.Full()) | squareBox | wideBox | widestBox
            );

        var alignValues = (Align[])Enum.GetValues(typeof(Align));

        var header = new object[] { null!, Text.Monospaced("Layout.Vertical()"), Text.Monospaced("Layout.Horizontal()") };

        var values = alignValues.Select(e => new[]
        {
            Text.Monospaced("Align." + e),
            AlignVerticalTest(e),
            AlignHorizontalTest(e)
        }).SelectMany(e => e).ToArray();

        return Layout.Grid().Columns(3)
               | (object[])[..header, ..values];
    }
}
```
