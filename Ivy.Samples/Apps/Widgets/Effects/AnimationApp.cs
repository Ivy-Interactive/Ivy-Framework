using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Effects;

[App(icon: Icons.Play)]
public class AnimationApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(4)
            .Padding(8)
            .Width(Size.Full())
            .Align(Align.Center)
            | CreateHeader()
            | CreateLoadingAnimations()
            | CreateBasicAnimations()
            | CreateInteractiveAnimations()
            | CreateAdvancedAnimations()
            | CreateIconAnimations()
            | CreateButtonAnimations();
    }

    private object CreateHeader()
    {
        return Layout.Vertical()
            .Gap(2)
            .Align(Align.Center)
            | Text.H1("Animation Showcase")
            | Text.Muted("Explore different animation types, triggers, and effects with interactive examples")
            | new Separator();
    }

    private object CreateLoadingAnimations()
    {
        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Loading & Processing Animations")
            | Text.Muted("Common animations for loading states, processing, and status indicators")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())
            | Icons.LoaderCircle.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Rotate).Duration(1)
            | Icons.Loader.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Duration(0.8)
            | Icons.LoaderPinwheel.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Pulse).Duration(1.2)
            | Icons.RefreshCw.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Rotate).Duration(1.5)
            | Icons.RefreshCcwDot.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Rotate).Duration(1.3)
            | Icons.Clock.ToIcon().Color(Colors.Gray).WithAnimation(AnimationType.Pulse).Duration(2)
            | Icons.Timer.ToIcon().Color(Colors.Amber).WithAnimation(AnimationType.Pulse).Duration(1.5)
            | Icons.Hourglass.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Rotate).Duration(2.5)
            | Icons.Activity.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Duration(1.2)
            | Icons.Signal.ToIcon().Color(Colors.Cyan).WithAnimation(AnimationType.Pulse).Duration(0.8)
            | Icons.Wifi.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Pulse).Duration(1.8);
    }

    private object CreateBasicAnimations()
    {
        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Basic Animations")
            | Text.Muted("Simple animations that run automatically")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())
            | Icons.Heart.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Pulse).Duration(2)
            | Icons.Zap.ToIcon().Color(Colors.Yellow).WithAnimation(AnimationType.Bounce).Duration(1.5)
            | Icons.Star.ToIcon().Color(Colors.Yellow).WithAnimation(AnimationType.Pulse).Duration(2)
            | Icons.Sparkles.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Shake).Duration(1)
            | Icons.Circle.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(2)
            | Icons.Timer.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Pulse).Duration(1.5)
            | Icons.Hourglass.ToIcon().Color(Colors.Amber).WithAnimation(AnimationType.Pulse).Duration(2.5)
            | Icons.Activity.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Duration(1.2)
            | Icons.Signal.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Bounce).Duration(0.8)
            | Icons.Wifi.ToIcon().Color(Colors.Cyan).WithAnimation(AnimationType.Pulse).Duration(1.8);
    }

    private object CreateInteractiveAnimations()
    {
        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Interactive Animations")
            | Text.Muted("Animations triggered by hover or click")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())
            | Icons.MousePointer.ToIcon().WithAnimation(AnimationType.Hover).Trigger(AnimationTrigger.Hover)
            | Icons.Target.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Hover)
            | Icons.Bell.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Shake).Trigger(AnimationTrigger.Click)
            | Icons.Gift.ToIcon().Color(Colors.Pink).WithAnimation(AnimationType.Bounce).Trigger(AnimationTrigger.Hover)
            | Icons.Rocket.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Bounce).Trigger(AnimationTrigger.Click)
            | Icons.Crown.ToIcon().Color(Colors.Yellow).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Hover)
            | Icons.Download.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Hover)
            | Icons.Upload.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Hover)
            | Icons.Check.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Click)
            | Icons.X.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Shake).Trigger(AnimationTrigger.Click)
            | Icons.CircleAlert.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Pulse).Trigger(AnimationTrigger.Hover);
    }

    private object CreateAdvancedAnimations()
    {
        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Advanced Animations")
            | Text.Muted("Complex animations with custom settings")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())

            | Icons.Flame.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Pulse).Duration(0.8).Intensity(1.5)
            | Icons.Wind.ToIcon().Color(Colors.Cyan).WithAnimation(AnimationType.Pulse).Duration(1.2).Intensity(0.8)

            | Icons.Moon.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(2)
            | Icons.Cloud.ToIcon().Color(Colors.Gray).WithAnimation(AnimationType.Pulse).Duration(2).Intensity(0.8)
            | Icons.Database.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(2).Intensity(0.8)

            | Icons.Cpu.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Pulse).Duration(1.5).Intensity(1.2)
            | Icons.MemoryStick.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Bounce).Duration(1).Intensity(0.7);
    }

    private object CreateIconAnimations()
    {
        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Icon Animations")
            | Text.Muted("Different icons with various animation effects")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())
            | Icons.Play.ToIcon().Color(Colors.Green).WithAnimation(AnimationType.Pulse).Duration(1.5)
            | Icons.Pause.ToIcon().Color(Colors.Orange).WithAnimation(AnimationType.Pulse).Duration(1)
            | Icons.SkipBack.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(1.2)
            | Icons.SkipForward.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(1.2)
            | Icons.Volume.ToIcon().Color(Colors.Purple).WithAnimation(AnimationType.Bounce).Duration(1.2)
            | Icons.VolumeX.ToIcon().Color(Colors.Red).WithAnimation(AnimationType.Shake).Duration(0.8)

            | Icons.Repeat.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse).Duration(1.5)
            | Icons.Music.ToIcon().Color(Colors.Pink).WithAnimation(AnimationType.Pulse).Duration(1.8)
            | Icons.Headphones.ToIcon().Color(Colors.Gray).WithAnimation(AnimationType.Hover).Trigger(AnimationTrigger.Hover);
    }

    private object CreateButtonAnimations()
    {
        var client = UseService<IClientProvider>();

        return Layout.Vertical()
            .Gap(3)
            .Width(Size.Full())
            .Align(Align.Center)
            | Text.H2("Button Animations")
            | Text.Muted("Interactive buttons with animation effects")
            | Layout.Wrap()
                .Gap(4)
                .Align(Align.Center)
                .Width(Size.Full())

            | new Button("Bounce", onClick: _ => client.Toast("Bouncing button clicked!"))
                .Icon(Icons.Zap)
                .WithAnimation(AnimationType.Bounce)
                .Trigger(AnimationTrigger.Click)
            | new Button("Pulse", onClick: _ => client.Toast("Pulsing button clicked!"))
                .Icon(Icons.Heart)
                .WithAnimation(AnimationType.Pulse)
                .Trigger(AnimationTrigger.Hover)
            | new Button("Shake", onClick: _ => client.Toast("Shaking button clicked!"))
                .Icon(Icons.Bell)
                .WithAnimation(AnimationType.Shake)
                .Trigger(AnimationTrigger.Click)
            | new Button("Zoom", onClick: _ => client.Toast("Zooming button clicked!"))
                .Icon(Icons.Search)
                .WithAnimation(AnimationType.Pulse)
                .Trigger(AnimationTrigger.Hover)
            | new Button("Slide", onClick: _ => client.Toast("Sliding button clicked!"))
                .Icon(Icons.ArrowRight)
                .WithAnimation(AnimationType.Pulse)
                .Trigger(AnimationTrigger.Hover);
    }
}