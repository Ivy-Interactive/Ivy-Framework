
namespace Ivy.Samples.Shared.Apps.Widgets.Effects;

[App(icon: Icons.PartyPopper, searchHints: ["celebration", "particles", "animation", "effects", "visual", "party"])]
public class ConfettiApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var onClick = new Baton("Click").OnClick(() =>
        {
            client.Toast("Did you see the confetti?");
        }).WithConfetti(AnimationTrigger.Click);

        var onHover = new Baton("Hover").WithConfetti(AnimationTrigger.Hover);

        var onAuto = new Baton("Auto").WithConfetti(AnimationTrigger.Auto);

        return Layout.Horizontal().Align(Align.Center).Height(Size.Screen()).Gap(20)
               | onClick
               | onHover
               | onAuto
            ;

    }
}
