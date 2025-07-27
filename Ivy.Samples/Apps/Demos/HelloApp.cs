using Ivy.Shared;

namespace Ivy.Samples.Apps.Demos;

[App(icon: Icons.PartyPopper, title: "Hello")]
public class HelloApp : ViewBase
{
    public override object? Build()
    {
        var nameState = this.UseState<string>();

        return Layout.Center()
               | (Layout.Vertical().Size(100)
                  | new Confetti(new IvyLogo())
                  | Text.H1("Hello " + nameState.Value)
                  | Text.Block("Welcome to the fantastic world of Ivy. Let's build something amazing together!")
                  | nameState.ToInput(placeholder: "What is your name?")
                  | new Separator()
                  | Text.Markdown("You'd be a hero to us if you could ⭐ us on [Github](https://github.com/Ivy-Interactive/Ivy-Framework)"))
            ;
    }
}