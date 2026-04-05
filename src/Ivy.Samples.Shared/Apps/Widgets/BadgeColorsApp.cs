namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Palette, group: ["Widgets"])]
public class BadgeColorsApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(8)
            | Text.H1("Badge Colors")
            | Text.P("Ivy supports a wide range of colors for badges, including semantic themes and custom CSS colors.")

            | new Card("Semantic Themes").Content(
                Layout.Horizontal().Wrap().Gap(2)
                    | new Badge("Primary").Primary()
                    | new Badge("Secondary").Secondary()
                    | new Badge("Destructive").Destructive()
                    | new Badge("Success").Success()
                    | new Badge("Warning").Warning()
                    | new Badge("Info").Info()
                    | new Badge("Muted").Muted()
                    | new Badge("Outline").Outline()
            )

            | new Card("Chromatic Colors").Content(
                Layout.Horizontal().Wrap().Gap(2)
                    | new Badge("Slate").Color(Colors.Slate)
                    | new Badge("Gray").Color(Colors.Gray)
                    | new Badge("Zinc").Color(Colors.Zinc)
                    | new Badge("Neutral").Color(Colors.Neutral)
                    | new Badge("Stone").Color(Colors.Stone)
                    | new Badge("Red").Color(Colors.Red)
                    | new Badge("Orange").Color(Colors.Orange)
                    | new Badge("Amber").Color(Colors.Amber)
                    | new Badge("Yellow").Color(Colors.Yellow)
                    | new Badge("Lime").Color(Colors.Lime)
                    | new Badge("Green").Color(Colors.Green)
                    | new Badge("Emerald").Color(Colors.Emerald)
                    | new Badge("Teal").Color(Colors.Teal)
                    | new Badge("Cyan").Color(Colors.Cyan)
                    | new Badge("Sky").Color(Colors.Sky)
                    | new Badge("Blue").Color(Colors.Blue)
                    | new Badge("Indigo").Color(Colors.Indigo)
                    | new Badge("Violet").Color(Colors.Violet)
                    | new Badge("Purple").Color(Colors.Purple)
                    | new Badge("Fuchsia").Color(Colors.Fuchsia)
                    | new Badge("Pink").Color(Colors.Pink)
                    | new Badge("Rose").Color(Colors.Rose)
                    | new Badge("Ivy Green").Color(Colors.IvyGreen)
            )

            | new Card("Custom Colors").Content(
                Layout.Vertical().Gap(4)
                    | Text.P("You can use any valid CSS color string (Hex, RGB, HSL).")
                    | (Layout.Horizontal().Wrap().Gap(2)
                        | new Badge("Custom Rose 500").Color("#f43f5e")
                        | new Badge("Custom Teal 400").Color("#2dd4bf")
                        | new Badge("Custom Indigo 600").Color("#4f46e5")
                        | new Badge("Custom RGB").Color("rgb(255, 99, 71)")
                        | new Badge("Custom HSL").Color("hsl(200, 100%, 50%)"))
            )

            | new Card("Icons & Styling").Content(
                Layout.Horizontal().Wrap().Gap(2)
                    | new Badge("With Icon", icon: Icons.Rocket).Primary()
                    | new Badge("Small", icon: Icons.Activity).Small().Success()
                    | new Badge("Large", icon: Icons.Zap).Large().Warning()
            );
    }
}
