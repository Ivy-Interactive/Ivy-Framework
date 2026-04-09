namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Palette, group: ["Tests"], isVisible: false, searchHints: ["badge", "color", "semantic", "enum", "ivygreen"])]
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

    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Badge Colors Test")
               | Text.P("Examples for semantic colors and enum colors.")
               | Text.H2("Semantic Colors (Success, Warning, etc.)")
               | Layout.Wrap(
                   SemanticColors.Select(x => new Badge(x.Label).Color(x.Color))
               )
               | Text.H2("Custom Colors via Colors Enum (Colors.Cyan, etc.)")
               | Layout.Wrap(
                   EnumColors.Select(x => new Badge(x.Label).Color(x.Color))
               )
               | Text.H2("API Usage")
               | (Layout.Horizontal()
                   | new Badge("Semantic via helper").Success()
                   | new Badge("Enum via Color()").Color(Colors.Cyan)
                   | new Badge("Ivy Green via Color()").Color(Colors.IvyGreen));
    }
}
