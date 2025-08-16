using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.SquareChevronRight, path: ["Widgets"])]
public class ButtonApp() : SampleBase
{
    private static readonly ButtonVariant[] Variants = [
        ButtonVariant.Primary,
        ButtonVariant.Destructive,
        ButtonVariant.Secondary,
        ButtonVariant.Outline,
        ButtonVariant.Ghost,
        ButtonVariant.Link
    ];

    private static readonly string[] VariantNames = [
        "Primary",
        "Destructive",
        "Secondary",
        "Outline",
        "Ghost",
        "Link"
    ];

    protected override object? BuildSample()
    {
        var label = this.UseState("Click a button");

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
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Normal state
                  | Variants.Select(variant => new Button(VariantNames[Array.IndexOf(Variants, variant)], eventHandler, variant: variant)).ToArray()

                  // Disabled state
                  | Variants.Select(variant => new Button(VariantNames[Array.IndexOf(Variants, variant)], eventHandler, variant: variant).Disabled()).ToArray()

                  // Loading state
                  | Variants.Select(variant => new Button(VariantNames[Array.IndexOf(Variants, variant)], eventHandler, variant: variant).Loading()).ToArray()
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
               | (Layout.Grid().Columns(Variants.Length)
                  | VariantNames.Select(name => Text.Block(name)).ToArray()

                  // Icon Left
                  | Variants.Select(variant => new Button("Button With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX)).ToArray()

                  // Icon Right
                  | Variants.Select(variant => new Button("Button With Icon", eventHandler, variant: variant, icon: Icons.MessageSquareX).Icon(Icons.MessageSquareX, Align.Right)).ToArray()
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

               | Text.H2("Interactive Demo")
               | Text.Literal(label.Value)
            ;
    }
}