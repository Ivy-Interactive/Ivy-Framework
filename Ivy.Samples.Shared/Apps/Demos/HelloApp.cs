using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.PartyPopper, title: "Hello")]
public class HelloApp : ViewBase
{
    public override object? Build()
    {
        var nameState = this.UseState<string>();

        return Layout.Center()
               | (new Card(
                   Layout.Vertical().Gap(6).Padding(2)
                   | new Confetti(new IvyLogo())
                   | Text.H2("Hello " + (string.IsNullOrEmpty(nameState.Value) ? "there" : nameState.Value) + "!")
                   | Text.Block("Welcome to the fantastic world of Ivy. Let's build something amazing together!")
                   | nameState.ToInput(placeholder: "What is your name?")
                   | new Separator()
                   | Text.Markdown("You'd be a hero to us if you could ⭐ us on [Github](https://github.com/Ivy-Interactive/Ivy-Framework)")
                 )
                 .Width(Size.Units(120).Max(500)));
    }
}