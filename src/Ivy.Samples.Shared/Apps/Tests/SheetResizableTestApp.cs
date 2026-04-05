namespace Ivy.Samples.Shared.Apps.Tests;

/// <summary>
/// Manual scenarios for Sheet with <see cref="SheetExtensions.Resizable"/> and <see cref="WidgetBase.Width"/> / <see cref="WidgetBase.Height"/>.
/// Use this to verify the sheet panel size and drag limits match the declared Size (including min/max).
/// </summary>
[App(
    icon: Icons.PanelRight,
    group: ["Tests"],
    isVisible: true,
    searchHints: ["sheet", "resizable", "width", "height", "drawer", "panel", "resize"])]
public class SheetResizableTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var plainWidth = UseState(false);
        var resizableDefault = UseState(false);
        var widthThenResizable = UseState(false);
        var resizableThenWidth = UseState(false);
        var customConstraints = UseState(false);
        var topResizableHeight = UseState(false);

        static object SheetBody(string label) =>
            Layout.Vertical().Gap(2)
                | Text.H4(label)
                | Text.P(
                    "Drag the inner edge to resize when Resizable is enabled. "
                        + "Initial width/height should match the button label unless noted.");

        return new Fragment(
            Layout.Vertical().Gap(4)
                | Text.H3("Resizable sheet + Width / Height")
                | Text.P(
                    "Covers default sizing, explicit Width, and Min/Max constraints. "
                        + "Order matters: prefer .Width(...).Resizable() so constraints stay attached to the same Size.")
                | Layout.Wrap(
                        new Button("Width only (Px 480), not resizable", _ => plainWidth.Set(true)),
                        new Button("Resizable (defaults)", _ => resizableDefault.Set(true)),
                        new Button("Width Px 480 → Resizable", _ => widthThenResizable.Set(true)),
                        new Button("Resizable → Width Px 480", _ => resizableThenWidth.Set(true)),
                        new Button("Rem 20 + Min/Max → Resizable", _ => customConstraints.Set(true)),
                        new Button("Top sheet: Height + Resizable", _ => topResizableHeight.Set(true)))
                    .Gap(2),

            plainWidth.Value
                ? new Sheet(
                    onClose: () => plainWidth.Set(false),
                    content: SheetBody("Fixed width 480px (not resizable)"),
                    title: "Width only",
                    description: "Sheet.Width(Size.Px(480)), Resizable false"
                ).Width(Size.Px(480))
                : null,
            resizableDefault.Value
                ? new Sheet(
                    onClose: () => resizableDefault.Set(false),
                    content: SheetBody("Resizable with default width (Rem 24 + default min/max)"),
                    title: "Resizable defaults",
                    description: "Resizable() only"
                ).Resizable()
                : null,
            widthThenResizable.Value
                ? new Sheet(
                    onClose: () => widthThenResizable.Set(false),
                    content: SheetBody("Initial width should be 480px; drag within min/max"),
                    title: "Width then Resizable",
                    description: "Width(Size.Px(480)).Resizable()"
                )
                    .Width(Size.Px(480))
                    .Resizable()
                : null,
            resizableThenWidth.Value
                ? new Sheet(
                    onClose: () => resizableThenWidth.Set(false),
                    content: SheetBody(
                        "If width looks wrong, Width() may have replaced the Size chain after Resizable()."),
                    title: "Resizable then Width",
                    description: "Resizable().Width(Size.Px(480)) — order-sensitive"
                )
                    .Resizable()
                    .Width(Size.Px(480))
                : null,
            customConstraints.Value
                ? new Sheet(
                    onClose: () => customConstraints.Set(false),
                    content: SheetBody("Resize between 300px and 600px; initial ~20rem"),
                    title: "Custom min / max",
                    description: "Width(Size.Rem(20).Min(Size.Px(300)).Max(Size.Px(600))).Resizable()"
                )
                    .Width(Size.Rem(20).Min(Size.Px(300)).Max(Size.Px(600)))
                    .Resizable()
                : null,
            topResizableHeight.Value
                ? new Sheet(
                    onClose: () => topResizableHeight.Set(false),
                    content: SheetBody("Top sheet: resize vertical edge; initial height 14rem"),
                    title: "Top + Height + Resizable",
                    description: "Side(Top).Height(...).Resizable()"
                )
                    .Side(SheetSide.Top)
                    .Resizable()
                : null);
    }
}
