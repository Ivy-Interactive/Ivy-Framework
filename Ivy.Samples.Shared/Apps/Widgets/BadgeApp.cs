using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Pill, path: ["Widgets"])]
public class BadgeApp : SampleBase
{
    private static readonly BadgeVariant[] Variants = [
        BadgeVariant.Primary,
        BadgeVariant.Destructive,
        BadgeVariant.Secondary,
        BadgeVariant.Outline
    ];

    private static readonly string[] VariantNames = [
        "Primary",
        "Destructive",
        "Secondary",
        "Outline"
    ];

    protected override object? BuildSample()
    {
        var createBadgeRow = (Func<BadgeVariant, Badge> badgeFactory) =>
            Layout.Grid().Columns(Variants.Length)
            | VariantNames.Select(name => Text.Block(name)).ToArray()
            | Variants.Select(badgeFactory).ToArray();

        return Layout.Vertical()
               | Text.H1("Badges")
               | Text.H2("Variants")
               | createBadgeRow(variant => new Badge(VariantNames[Array.IndexOf(Variants, variant)], variant: variant))

               | Text.H2("Sizes")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Small
                  | Variants.Select(variant => new Badge("Small", variant: variant).Small()).ToArray()

                  // Medium
                  | Variants.Select(variant => new Badge("Medium", variant: variant)).ToArray()

                  // Large
                  | Variants.Select(variant => new Badge("Large", variant: variant).Large()).ToArray()
               )

               | Text.H2("With Icons")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Bell icon
                  | Variants.Select(variant => new Badge("With Bell", variant: variant, icon: Icons.Bell)).ToArray()

                  // Heart icon
                  | Variants.Select(variant => new Badge("With Heart", variant: variant, icon: Icons.Heart)).ToArray()

                  // Star icon
                  | Variants.Select(variant => new Badge("With Star", variant: variant, icon: Icons.Star)).ToArray()

                  // Check icon
                  | Variants.Select(variant => new Badge("With Check", variant: variant, icon: Icons.Check)).ToArray()
               )

               | Text.H2("Icon Positioning")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Icon on left (default)
                  | Variants.Select(variant => new Badge("Left Icon", variant: variant).Icon(Icons.Bell, Align.Left)).ToArray()

                  // Icon on right
                  | Variants.Select(variant => new Badge("Right Icon", variant: variant).Icon(Icons.ArrowRight, Align.Right)).ToArray()
               )

               | Text.H2("Icon Only")
               | Layout.Horizontal(
                   new Badge(icon: Icons.Bell),
                   new Badge(icon: Icons.Heart, variant: BadgeVariant.Destructive),
                   new Badge(icon: Icons.Star, variant: BadgeVariant.Outline),
                   new Badge(icon: Icons.Check, variant: BadgeVariant.Secondary)
               )
            ;
    }
}