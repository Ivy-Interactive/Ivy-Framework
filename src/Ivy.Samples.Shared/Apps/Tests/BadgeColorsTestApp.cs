namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Palette, group: ["Tests"], isVisible: false, searchHints: ["badge", "color", "semantic", "hex", "rgb", "hsl"])]
public class BadgeColorsTestApp : SampleBase
{
    private static readonly (string Label, Colors Color)[] SemanticColors = [
        ("Success", Colors.Success),
        ("Warning", Colors.Warning),
        ("Info", Colors.Info),
        ("Destructive", Colors.Destructive),
        ("Primary", Colors.Primary),
        ("Secondary", Colors.Secondary),
        ("Muted", Colors.Muted),
        ("IvyGreen", Colors.IvyGreen)
    ];

    private static readonly (string Label, Colors Color)[] EnumColors = [
        ("Cyan", Colors.Cyan),
        ("Blue", Colors.Blue),
        ("Violet", Colors.Violet),
        ("Rose", Colors.Rose),
        ("Amber", Colors.Amber),
        ("Emerald", Colors.Emerald),
        ("Slate", Colors.Slate)
    ];

    private static readonly (string Label, string Value)[] StringColors = [
        ("Hex #22C55E", "#22C55E"),
        ("Hex #06B6D4", "#06B6D4"),
        ("RGB 245, 158, 11", "rgb(245, 158, 11)"),
        ("RGB 99, 102, 241", "rgb(99, 102, 241)"),
        ("HSL 142, 71%, 45%", "hsl(142, 71%, 45%)"),
        ("HSL 332, 84%, 48%", "hsl(332, 84%, 48%)")
    ];

    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Badge Colors Test")
               | Text.P("Examples for semantic colors, enum colors, and string colors.")
               | Text.H2("Semantic Colors (Success, Warning, etc.)")
               | Layout.Wrap(
                   SemanticColors.Select(x => new Badge(x.Label).Color(x.Color))
               )
               | Text.H2("Custom Colors via Colors Enum (Colors.Cyan, etc.)")
               | Layout.Wrap(
                   EnumColors.Select(x => new Badge(x.Label).Color(x.Color))
               )
               | Text.H2("Color via String Value (Hex, RGB, HSL)")
               | Layout.Wrap(
                   StringColors.Select(x => new Badge(x.Label).Color(x.Value))
               )
               | Text.H2("API Usage")
               | (Layout.Horizontal()
                   | new Badge("Semantic via helper").Success()
                   | new Badge("Enum via Color()").Color(Colors.Cyan)
                   | new Badge("Hex via Color()").Color("#22C55E")
                   | new Badge("RGB via Color()").Color("rgb(99, 102, 241)")
                   | new Badge("HSL via Color()").Color("hsl(332, 84%, 48%)"));
    }
}
