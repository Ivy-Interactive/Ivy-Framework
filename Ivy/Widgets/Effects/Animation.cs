using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum AnimationType
{
    Rotate,
    SlideIn,
    FadeIn,
    ZoomIn,
    SlideOut,
    FadeOut,
    ZoomOut,
    Bounce,
    Shake,
    Flip,
    Stagger,
    Wave,
    Pulse,
    Spring,
    Hover
}

public enum AnimationDirection
{
    Left,
    Right,
    Up,
    Down
}

public enum AnimationEasing
{
    EaseIn,
    EaseOut,
    EaseInOut,
    Linear,
    CircIn,
    CircOut,
    CircInOut,
    BackIn,
    BackOut,
    BackInOut,
    Anticipate,
    AnticipateOut,
    BounceIn,
    BounceOut,
    BounceInOut,
    ElasticIn,
    ElasticOut,
    ElasticInOut
}

public enum AnimationTrigger
{
    Auto,
    Click,
    Hover
}

public record Animation : WidgetBase<Animation>
{
    public Animation(AnimationType animation)
    {
        Type = animation;
    }

    public Animation() { }

    [Prop] public AnimationType Type { get; set; } = AnimationType.Rotate;

    [Prop] public double Duration { get; set; } = 0.5;

    [Prop] public double Delay { get; set; }

    [Prop] public AnimationDirection? Direction { get; set; }

    [Prop] public double Distance { get; set; } = 100;

    [Prop] public AnimationEasing? Easing { get; set; } = AnimationEasing.Linear;

    [Prop] public int? Repeat { get; set; } = null;

    [Prop] public double RepeatDelay { get; set; }

    [Prop] public double Intensity { get; set; } = 1;

    [Prop] public AnimationTrigger Trigger { get; set; } = AnimationTrigger.Auto;
}

public static class AnimationExtensions
{
    public static Animation WithAnimation(this IWidget widget, AnimationType animation) => new Animation(animation).Content(widget);

    public static Animation Duration(this Animation animation, double duration) => animation with { Duration = duration };

    public static Animation Delay(this Animation animation, double delay) => animation with { Delay = delay };

    public static Animation Direction(this Animation animation, AnimationDirection direction) => animation with { Direction = direction };

    public static Animation Distance(this Animation animation, double distance) => animation with { Distance = distance };

    public static Animation Easing(this Animation animation, AnimationEasing easing) => animation with { Easing = easing };

    public static Animation Repeat(this Animation animation, int? repeat) => animation with { Repeat = repeat };

    public static Animation RepeatDelay(this Animation animation, double repeatDelay) => animation with { RepeatDelay = repeatDelay };

    public static Animation Intensity(this Animation animation, double intensity) => animation with { Intensity = intensity };

    public static Animation Trigger(this Animation animation, AnimationTrigger trigger) => animation with { Trigger = trigger };

    public static Animation Content(this Animation animation, object child) => animation with { Children = [child] };
}