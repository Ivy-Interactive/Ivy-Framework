using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Effects;

[App(icon: Icons.PartyPopper)]
public class ConfettiApp : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        var onClick = new Button("Click").HandleClick(() =>
        {
            client.Toast("Did you see the confetti?");
        }).WithConfetti(AnimationTrigger.Click);

        var onHover = new Button("Hover").WithConfetti(AnimationTrigger.Hover);

        var onAuto = new Button("Auto").WithConfetti(AnimationTrigger.Auto);

        return Layout.Horizontal().Align(Align.Center).Height(Size.Screen()).Gap(20)
               | onClick
               | onHover
               | onAuto
            ;

    }
}