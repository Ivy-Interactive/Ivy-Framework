
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.SquareChevronRight, group: ["Widgets"], searchHints: ["click", "action", "submit", "cta", "interactive", "control"])]
public class BatonApp() : SampleBase
{
    private static readonly BatonVariant[] Variants = [
        BatonVariant.Primary,
        BatonVariant.Destructive,
        BatonVariant.Secondary,
        BatonVariant.Success,
        BatonVariant.Warning,
        BatonVariant.Info,
        BatonVariant.Outline,
        BatonVariant.Ghost,
        BatonVariant.Link,
    ];

    private static readonly string[] VariantNames = [
        "Primary",
        "Destructive",
        "Secondary",
        "Success",
        "Warning",
        "Info",
        "Outline",
        "Ghost",
        "Link",
    ];

    protected override object? BuildSample()
    {
        var label = UseState("Click a button");

        var eventHandler = (Event<Baton> e) =>
        {
            label.Set($"Baton {e.Sender.Title} was clicked.");
        };

        var createBatonRow = (Func<BatonVariant, Baton> buttonFactory) =>
            Layout.Grid().Columns(Variants.Length)
            | VariantNames.Select(name => Text.Block(name)).ToArray()
            | Variants.Select(buttonFactory).ToArray();

        return Layout.Vertical()
               | Text.H1("Batons")
               | Text.H2("Variants")
               | createBatonRow(variant => new Baton(VariantNames[Array.IndexOf(Variants, variant)], eventHandler, variant: variant))

               | Text.H2("States")
               | (Layout.Wrap().Gap(16)
                  | Variants.Select((variant, idx) =>
                      Layout.Vertical()
                      .Width(Size.MinContent())
                 | Text.Block(VariantNames[idx])
                 | new Baton(VariantNames[idx], eventHandler, variant: variant)                     // Normal
                 | new Baton(VariantNames[idx], eventHandler, variant: variant).Disabled()          // Disabled
                 | new Baton(VariantNames[idx], eventHandler, variant: variant).Loading()           // Loading
                  ).ToArray()
)

               | Text.H2("Sizes")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Small
                  | Variants.Select(variant => new Baton("Small", eventHandler, variant: variant).Small()).ToArray()

                  // Medium
                  | Variants.Select(variant => new Baton("Medium", eventHandler, variant: variant)).ToArray()

                  // Large
                  | Variants.Select(variant => new Baton("Large", eventHandler, variant: variant).Large()).ToArray()
               )

               | Text.H2("With Icons")
               | (Layout.Wrap().Gap(16)
                  | Variants.Select((variant, idx) =>
                    Layout.Vertical()
                    .Width(Size.MinContent())
               | Text.Block(VariantNames[idx])
               | new Baton("Baton With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX)
               | new Baton("Baton With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX).Icon(Icons.MessageSquareX, Align.Right)
                ).ToArray()
)


               | Text.H2("Styling")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Rounded
                  | Variants.Select(variant => new Baton("Rounded", eventHandler, variant: variant).BorderRadius(BorderRadius.Rounded)).ToArray()

                  // Full
                  | Variants.Select(variant => new Baton("Full", eventHandler, variant: variant).BorderRadius(BorderRadius.Full)).ToArray()

                  // With Tooltip
                  | Variants.Select(variant => new Baton("With Tooltip", eventHandler, variant: variant).Tooltip("This is a tooltip!")).ToArray()
               )

               | Text.H2("Icon Only")
               | Layout.Horizontal(
                   Icons.MessageSquareX.ToBaton(eventHandler),
                   Icons.Heart.ToBaton(eventHandler, BatonVariant.Destructive),
                   Icons.Star.ToBaton(eventHandler, BatonVariant.Outline)
               )
               | Layout.Horizontal(
                   Icons.MessageSquareX.ToBaton(eventHandler).Small(),
                   Icons.Heart.ToBaton(eventHandler, BatonVariant.Destructive).Small(),
                   Icons.Star.ToBaton(eventHandler, BatonVariant.Outline).Small()
               )

               | Text.H2("Batons with URLs")
               | (Layout.Horizontal().Gap(8)
                   | new Baton("Visit Ivy GitHub", variant: BatonVariant.Primary)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
                   | new Baton("External Link", variant: BatonVariant.Secondary)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
                       .Icon(Icons.ExternalLink, Align.Right)
                   | new Baton("Link Style", variant: BatonVariant.Link)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
               )

               | Text.H2("AI Baton")
               | (Layout.Horizontal().Gap(8)
                   | new Baton("AI Baton", eventHandler, variant: BatonVariant.Ai)
                   | new Baton("With Icon", eventHandler, variant: BatonVariant.Ai).Icon(Icons.Sparkles)
               )
               | (Layout.Horizontal().Gap(8)
                   | new Baton("Small", eventHandler, variant: BatonVariant.Ai).Small()
                   | new Baton("Large", eventHandler, variant: BatonVariant.Ai).Large()
                   | new Baton("Full Rounded", eventHandler, variant: BatonVariant.Ai).BorderRadius(BorderRadius.Full)
               )

               | Text.H2("Interactive Demo")
               | Text.Literal(label.Value)
            ;
    }
}
