using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the types of animations that can be applied to widgets.
/// Each animation type provides a different visual effect for enhancing user interface interactions.
/// </summary>
public enum AnimationType
{
    /// <summary>Continuous rotation animation around the center point.</summary>
    Rotate,
    /// <summary>Element slides into view from the specified direction.</summary>
    SlideIn,
    /// <summary>Element gradually appears by increasing opacity from 0 to 1.</summary>
    FadeIn,
    /// <summary>Element scales up from a smaller size to its normal size.</summary>
    ZoomIn,
    /// <summary>Element slides out of view in the specified direction.</summary>
    SlideOut,
    /// <summary>Element gradually disappears by decreasing opacity from 1 to 0.</summary>
    FadeOut,
    /// <summary>Element scales down from its normal size to a smaller size.</summary>
    ZoomOut,
    /// <summary>Element bounces up and down with elastic motion.</summary>
    Bounce,
    /// <summary>Element shakes horizontally or vertically with rapid movement.</summary>
    Shake,
    /// <summary>Element flips around its axis creating a 3D rotation effect.</summary>
    Flip,
    /// <summary>Sequential animation where child elements animate with delays.</summary>
    Stagger,
    /// <summary>Smooth wave-like motion across the element.</summary>
    Wave,
    /// <summary>Element pulses by scaling slightly larger and smaller repeatedly.</summary>
    Pulse,
    /// <summary>Elastic spring-like animation with overshoot and settle.</summary>
    Spring,
    /// <summary>Animation triggered on hover interactions.</summary>
    Hover
}

/// <summary>
/// Defines the directional movement for animations that support directional motion.
/// Used with slide, shake, and other movement-based animations.
/// </summary>
public enum AnimationDirection
{
    /// <summary>Animation moves toward or from the left side.</summary>
    Left,
    /// <summary>Animation moves toward or from the right side.</summary>
    Right,
    /// <summary>Animation moves toward or from the top.</summary>
    Up,
    /// <summary>Animation moves toward or from the bottom.</summary>
    Down
}

/// <summary>
/// Defines easing functions that control the acceleration and deceleration of animations.
/// Easing functions determine how the animation progresses over time, creating different feels and personalities.
/// </summary>
public enum AnimationEasing
{
    /// <summary>Animation starts slowly and accelerates toward the end.</summary>
    EaseIn,
    /// <summary>Animation starts quickly and decelerates toward the end.</summary>
    EaseOut,
    /// <summary>Animation starts slowly, accelerates in the middle, then decelerates at the end.</summary>
    EaseInOut,
    /// <summary>Animation progresses at a constant speed throughout.</summary>
    Linear,
    /// <summary>Circular easing that starts slowly and accelerates.</summary>
    CircIn,
    /// <summary>Circular easing that starts quickly and decelerates.</summary>
    CircOut,
    /// <summary>Circular easing that combines CircIn and CircOut.</summary>
    CircInOut,
    /// <summary>Back easing that pulls back before moving forward (anticipation).</summary>
    BackIn,
    /// <summary>Back easing that overshoots the target then settles back.</summary>
    BackOut,
    /// <summary>Back easing that combines BackIn and BackOut behaviors.</summary>
    BackInOut,
    /// <summary>Anticipation easing that pulls back before the main motion.</summary>
    Anticipate,
    /// <summary>Anticipation easing that overshoots then settles back.</summary>
    AnticipateOut,
    /// <summary>Bounce easing that starts with bouncing motion.</summary>
    BounceIn,
    /// <summary>Bounce easing that ends with bouncing motion.</summary>
    BounceOut,
    /// <summary>Bounce easing that combines BounceIn and BounceOut.</summary>
    BounceInOut,
    /// <summary>Elastic easing that starts with elastic oscillation.</summary>
    ElasticIn,
    /// <summary>Elastic easing that ends with elastic oscillation.</summary>
    ElasticOut,
    /// <summary>Elastic easing that combines ElasticIn and ElasticOut.</summary>
    ElasticInOut
}

/// <summary>
/// Defines when an animation should be triggered.
/// </summary>
public enum AnimationTrigger
{
    /// <summary>Animation starts automatically when the widget is rendered.</summary>
    Auto,
    /// <summary>Animation is triggered when the user clicks on the widget.</summary>
    Click,
    /// <summary>Animation is triggered when the user hovers over the widget.</summary>
    Hover
}

/// <summary>
/// Widget that applies visual animations to its child content.
/// </summary>
public record Animation : WidgetBase<Animation>
{
    /// <summary>
    /// Initializes a new instance of the Animation class.
    /// </summary>
    /// <param name="animation">The type of animation effect.</param>
    public Animation(AnimationType animation)
    {
        Type = animation;
    }

    /// <summary>
    /// Gets or sets the type of animation effect.
    /// </summary>
    [Prop] public AnimationType Type { get; set; }

    /// <summary>
    /// Gets or sets the duration of the animation in seconds.
    /// </summary>
    [Prop] public double Duration { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the delay before the animation starts in seconds.
    /// </summary>
    [Prop] public double Delay { get; set; }

    /// <summary>
    /// Gets or sets the direction for directional animations.
    /// </summary>
    [Prop] public AnimationDirection? Direction { get; set; }

    /// <summary>
    /// Gets or sets the distance for movement-based animations in pixels.
    /// </summary>
    [Prop] public double Distance { get; set; } = 100;

    /// <summary>
    /// Gets or sets the easing function that controls animation acceleration and deceleration.
    /// </summary>
    [Prop] public AnimationEasing? Easing { get; set; } = AnimationEasing.Linear;

    /// <summary>
    /// Gets or sets the number of times the animation should repeat.
    /// </summary>
    [Prop] public int? Repeat { get; set; } = null;

    /// <summary>
    /// Gets or sets the delay between animation repetitions in seconds.
    /// </summary>
    [Prop] public double RepeatDelay { get; set; }

    /// <summary>
    /// Gets or sets the intensity or magnitude of the animation effect.
    /// </summary>
    [Prop] public double Intensity { get; set; } = 1;

    /// <summary>
    /// Gets or sets when the animation should be triggered.
    /// </summary>
    [Prop] public AnimationTrigger Trigger { get; set; } = AnimationTrigger.Auto;
}

/// <summary>
/// Provides extension methods for creating and configuring Animation widgets with fluent syntax.
/// </summary>
public static class AnimationExtensions
{
    /// <summary>
    /// Wraps a widget with an animation effect, creating a fluent way to apply animations.
    /// </summary>
    /// <param name="widget">The widget to animate.</param>
    /// <param name="animation">The type of animation to apply.</param>
    /// <returns>An Animation widget containing the specified widget with the animation effect.</returns>
    public static Animation WithAnimation(this IWidget widget, AnimationType animation)
    {
        return new Animation(animation).Content(widget);
    }

    /// <summary>
    /// Sets the duration of the animation.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="duration">The duration in seconds.</param>
    /// <returns>The animation with the specified duration.</returns>
    public static Animation Duration(this Animation animation, double duration)
    {
        return animation with { Duration = duration };
    }

    /// <summary>
    /// Sets the delay before the animation starts.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="delay">The delay in seconds.</param>
    /// <returns>The animation with the specified delay.</returns>
    public static Animation Delay(this Animation animation, double delay)
    {
        return animation with { Delay = delay };
    }

    /// <summary>
    /// Sets the direction for directional animations.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="direction">The direction of movement.</param>
    /// <returns>The animation with the specified direction.</returns>
    public static Animation Direction(this Animation animation, AnimationDirection direction)
    {
        return animation with { Direction = direction };
    }

    /// <summary>
    /// Sets the distance for movement-based animations.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="distance">The distance in pixels.</param>
    /// <returns>The animation with the specified distance.</returns>
    public static Animation Distance(this Animation animation, double distance)
    {
        return animation with { Distance = distance };
    }

    /// <summary>
    /// Sets the easing function for the animation.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="easing">The easing function to apply.</param>
    /// <returns>The animation with the specified easing.</returns>
    public static Animation Easing(this Animation animation, AnimationEasing easing)
    {
        return animation with { Easing = easing };
    }

    /// <summary>
    /// Sets the number of times the animation should repeat.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="repeat">The number of repetitions, or null for no repetition.</param>
    /// <returns>The animation with the specified repeat count.</returns>
    public static Animation Repeat(this Animation animation, int? repeat)
    {
        return animation with { Repeat = repeat };
    }

    /// <summary>
    /// Sets the delay between animation repetitions.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="repeatDelay">The delay in seconds between repeats.</param>
    /// <returns>The animation with the specified repeat delay.</returns>
    public static Animation RepeatDelay(this Animation animation, double repeatDelay)
    {
        return animation with { RepeatDelay = repeatDelay };
    }

    /// <summary>
    /// Sets the intensity or magnitude of the animation effect.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="intensity">The intensity multiplier (1.0 is normal intensity).</param>
    /// <returns>The animation with the specified intensity.</returns>
    public static Animation Intensity(this Animation animation, double intensity)
    {
        return animation with { Intensity = intensity };
    }

    /// <summary>
    /// Sets when the animation should be triggered.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="trigger">The trigger condition for the animation.</param>
    /// <returns>The animation with the specified trigger.</returns>
    public static Animation Trigger(this Animation animation, AnimationTrigger trigger)
    {
        return animation with { Trigger = trigger };
    }

    /// <summary>
    /// Sets the content to be animated.
    /// </summary>
    /// <param name="animation">The animation to configure.</param>
    /// <param name="child">The widget or content to animate.</param>
    /// <returns>The animation with the specified content.</returns>
    public static Animation Content(this Animation animation, object child)
    {
        return animation with { Children = [child] };
    }
}