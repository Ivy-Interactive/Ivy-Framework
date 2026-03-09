using Ivy.Core;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Pill, path: ["Widgets"], searchHints: ["tag", "label", "chip", "status", "indicator", "pill"])]
public class BadgeApp : SampleBase
{
    private IClientProvider _client => UseService<IClientProvider>();

    private static readonly BadgeVariant[] Variants = [
        BadgeVariant.Primary,
        BadgeVariant.Destructive,
        BadgeVariant.Secondary,
        BadgeVariant.Outline,
        BadgeVariant.Success,
        BadgeVariant.Warning,
        BadgeVariant.Info
    ];

    private static readonly string[] VariantNames = [
        "Primary",
        "Destructive",
        "Secondary",
        "Outline",
        "Success",
        "Warning",
        "Info"
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
                   new Badge(icon: Icons.Check, variant: BadgeVariant.Secondary),
                   new Badge(icon: Icons.CircleCheck, variant: BadgeVariant.Success),
                   new Badge(icon: Icons.CircleAlert, variant: BadgeVariant.Warning),
                   new Badge(icon: Icons.Info, variant: BadgeVariant.Info),
                   new Badge("1"),
                   new Badge("2", variant: BadgeVariant.Destructive),
                   new Badge("3", variant: BadgeVariant.Outline),
                   new Badge("10", variant: BadgeVariant.Secondary),
                   new Badge("99+", variant: BadgeVariant.Success)
               )

               | Text.H2("Clickable Badges")
               | new ClickableBadgesExample()
            ;
    }
}

public class ClickableBadgesExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var popupMessage = UseState("");

        return Layout.Vertical()
            | Layout.Horizontal(
                new Badge("Click Me", icon: Icons.MousePointer)
                    .OnClick(_ => { popupMessage.Set("Badge clicked!"); isOpen.Set(true); }),
                new Badge("Filter", icon: Icons.ListFilterPlus, variant: BadgeVariant.Secondary)
                    .OnClick(_ => { popupMessage.Set("Filter applied!"); isOpen.Set(true); }),
                new Badge("Remove", icon: Icons.X, variant: BadgeVariant.Destructive)
                    .OnClick(_ => { popupMessage.Set("Item removed!"); isOpen.Set(true); })
            )
            | (isOpen.Value
                ? new Dialog(
                    _ => isOpen.Set(false),
                    new DialogHeader("Badge Action"),
                    new DialogBody(Text.P(popupMessage.Value)),
                    new DialogFooter(
                        new Button("Close", _ => isOpen.Set(false))
                    )
                )
                : null);
    }
}
