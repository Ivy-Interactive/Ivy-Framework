using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Effects;

[App(icon: Icons.Play)]
public class AnimationApp : ViewBase
{
    public override object? Build()
    {
        return Icons.LoaderCircle.ToIcon().WithAnimation(AnimationType.Rotate).Duration(1);
    }
}