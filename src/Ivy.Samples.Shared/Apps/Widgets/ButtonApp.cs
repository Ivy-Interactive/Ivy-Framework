
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.SquareChevronRight, group: ["Widgets"], searchHints: ["click", "action", "submit", "cta", "interactive", "control"])]
public class ButtonApp() : SampleBase
{
    private static readonly ButtonVariant[] Variants = [
        ButtonVariant.Primary,
        ButtonVariant.Destructive,
        ButtonVariant.Secondary,
        ButtonVariant.Success,
        ButtonVariant.Warning,
        ButtonVariant.Info,
        ButtonVariant.Outline,
        ButtonVariant.Ghost,
        ButtonVariant.Link,
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

        var eventHandler = (Event<Button> e) =>
        {
            label.Set($"Button {e.Sender.Title} was clicked.");
        };

        var createButtonRow = (Func<ButtonVariant, Button> buttonFactory) =>
            Layout.Grid().Columns(Variants.Length)
            | VariantNames.Select(name => Text.Block(name)).ToArray()
            | Variants.Select(buttonFactory).ToArray();

        return Layout.Vertical()
               | Text.H1("Buttons")
               | Text.H2("Variants")
               | createButtonRow(variant => new Button(VariantNames[Array.IndexOf(Variants, variant)], eventHandler, variant: variant))

               | Text.H2("States")
               | (Layout.Wrap().Gap(16)
                  | Variants.Select((variant, idx) =>
                      Layout.Vertical()
                      .Width(Size.MinContent())
                 | Text.Block(VariantNames[idx])
                 | new Button(VariantNames[idx], eventHandler, variant: variant)                     // Normal
                 | new Button(VariantNames[idx], eventHandler, variant: variant).Disabled()          // Disabled
                 | new Button(VariantNames[idx], eventHandler, variant: variant).Loading()           // Loading
                  ).ToArray()
)

               | Text.H2("Sizes")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Small
                  | Variants.Select(variant => new Button("Small", eventHandler, variant: variant).Small()).ToArray()

                  // Medium
                  | Variants.Select(variant => new Button("Medium", eventHandler, variant: variant)).ToArray()

                  // Large
                  | Variants.Select(variant => new Button("Large", eventHandler, variant: variant).Large()).ToArray()
               )

               | Text.H2("With Icons")
               | (Layout.Wrap().Gap(16)
                  | Variants.Select((variant, idx) =>
                    Layout.Vertical()
                    .Width(Size.MinContent())
               | Text.Block(VariantNames[idx])
               | new Button("Button With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX)
               | new Button("Button With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX).Icon(Icons.MessageSquareX, Align.Right)
                ).ToArray()
)


               | Text.H2("Styling")
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Rounded
                  | Variants.Select(variant => new Button("Rounded", eventHandler, variant: variant).BorderRadius(BorderRadius.Rounded)).ToArray()

                  // Full
                  | Variants.Select(variant => new Button("Full", eventHandler, variant: variant).BorderRadius(BorderRadius.Full)).ToArray()

                  // With Tooltip
                  | Variants.Select(variant => new Button("With Tooltip", eventHandler, variant: variant).Tooltip("This is a tooltip!")).ToArray()
               )

               | Text.H2("Icon Only")
               | Layout.Horizontal(
                   Icons.MessageSquareX.ToButton(eventHandler),
                   Icons.Heart.ToButton(eventHandler, ButtonVariant.Destructive),
                   Icons.Star.ToButton(eventHandler, ButtonVariant.Outline)
               )
               | Layout.Horizontal(
                   Icons.MessageSquareX.ToButton(eventHandler).Small(),
                   Icons.Heart.ToButton(eventHandler, ButtonVariant.Destructive).Small(),
                   Icons.Star.ToButton(eventHandler, ButtonVariant.Outline).Small()
               )

               | Text.H2("Buttons with URLs")
               | (Layout.Horizontal().Gap(8)
                   | new Button("Visit Ivy GitHub", variant: ButtonVariant.Primary)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
                   | new Button("External Link", variant: ButtonVariant.Secondary)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
                       .Icon(Icons.ExternalLink, Align.Right)
                   | new Button("Link Style", variant: ButtonVariant.Link)
                       .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
               )

               | Text.H2("AI Button")
               | (Layout.Horizontal().Gap(8)
                   | new Button("AI Button", eventHandler, variant: ButtonVariant.Ai)
                   | new Button("With Icon", eventHandler, variant: ButtonVariant.Ai).Icon(Icons.Sparkles)
               )
               | (Layout.Horizontal().Gap(8)
                   | new Button("Small", eventHandler, variant: ButtonVariant.Ai).Small()
                   | new Button("Large", eventHandler, variant: ButtonVariant.Ai).Large()
                   | new Button("Full Rounded", eventHandler, variant: ButtonVariant.Ai).BorderRadius(BorderRadius.Full)
               )

               | Text.H2("Keyboard Shortcuts")
               | (Layout.Horizontal().Gap(8)
                   | new Button("Search", eventHandler, variant: ButtonVariant.Primary).ShortcutKey("Ctrl+K")
                   | new Button("Save", eventHandler, variant: ButtonVariant.Secondary).ShortcutKey("Ctrl+S")
                   | new Button("Delete", eventHandler, variant: ButtonVariant.Destructive).ShortcutKey("Delete")
                   | new Button("Submit", eventHandler, variant: ButtonVariant.Success).ShortcutKey("Ctrl+Enter")
               )

               | Text.H2("Interactive Demo")
               | Text.Literal(label.Value)
            ;
    }
}
