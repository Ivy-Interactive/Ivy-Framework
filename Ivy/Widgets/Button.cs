using System.Reactive;
using System.Runtime.CompilerServices;
using Ivy.Apps;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ButtonVariant
{
    Primary,
    Destructive,
    Outline,
    Secondary,
    Success,
    Warning,
    Info,
    Ghost,
    Link,
    Inline,
}

/// <summary>Interactive button widget supporting multiple variants, sizes, icons, and states for user actions and navigation.</summary>
public record Button : WidgetBase<Button>
{
    [OverloadResolutionPriority(1)]
    public Button(string? title = null, Func<Event<Button>, ValueTask>? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick;
    }

    public Button(string? title = null, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick?.ToValueTask();
    }

    public Button(string? title = null, Action? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick == null ? null : (_ => { onClick(); return ValueTask.CompletedTask; });
    }

    [Prop] public string? Title { get; set; }

    [Prop] public ButtonVariant Variant { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public Align IconPosition { get; set; } = Align.Left;

    [Prop] public Colors? Foreground { get; set; }

    [Prop] public string? Url { get; set; }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Tooltip { get; set; }

    [Prop] public bool Loading { get; set; }

    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    [Event] public Func<Event<Button>, ValueTask>? OnClick { get; set; }

    public object? Tag { get; set; } //not a prop!

    /// <summary>Prevents adding children to Button using pipe operator.</summary>
    /// <param name="widget">The Button widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Button does not support children.</exception>
    public static Button operator |(Button widget, object child)
    {
        throw new NotSupportedException("Button does not support children.");
    }
}

/// <summary>Extension methods for Button widget providing fluent API for configuring appearance, behavior, and interactions.</summary>
public static class ButtonExtensions
{
    [OverloadResolutionPriority(1)]
    public static Button ToButton(this Icons icon, Func<Event<Button>, ValueTask>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick, icon: icon, variant: variant);
    }

    public static Button ToButton(this Icons icon, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick?.ToValueTask(), icon: icon, variant: variant);
    }

    /// <param name="trigger">Button that triggers content display.</param>
    /// <param name="action">Function creating content to display when triggered.</param>
    /// <returns>IView managing trigger button and conditional content display.</returns>
    public static IView ToTrigger(this Button trigger, Func<IState<bool>, object> action)
    {
        return new FuncView((context) =>
            {
                var isOpen = context.UseState(false);
                var clonedTrigger = trigger with
                {
                    OnClick = async @event =>
                    {
                        if (trigger.OnClick != null)
                        {
                            await trigger.OnClick(@event);
                        }
                        isOpen.Value = true;
                    }
                };
                return new Fragment(
                    clonedTrigger,
                    isOpen.Value ? action(isOpen) : null
                );
            }
        );
    }

    public static Button Title(this Button button, string title)
    {
        return button with { Title = title };
    }

    public static Button Url(this Button button, string url)
    {
        // Validate URL to prevent open redirect vulnerabilities
        var validatedUrl = Utils.ValidateLinkUrl(url);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {url}. Only safe URLs (http/https, relative paths, app://, anchors) are allowed.", nameof(url));
        }
        return button with { Url = validatedUrl };
    }

    public static Button Disabled(this Button button, bool disabled = true)
    {
        return button with { Disabled = disabled };
    }

    public static Button Icon(this Button button, Icons? icon, Align position = Align.Left)
    {
        return button with { Icon = icon, IconPosition = position };
    }

    public static Button Variant(this Button button, ButtonVariant variant)
    {
        return button with { Variant = variant };
    }

    public static Button Foreground(this Button button, Colors color)
    {
        return button with { Foreground = color };
    }

    public static Button Tooltip(this Button button, string tooltip)
    {
        return button with { Tooltip = tooltip };
    }

    public static Button Loading(this Button button, bool loading = true)
    {
        return button with { Loading = loading };
    }

    public static Button Loading(this Button button, IState<bool> loading)
    {
        return button.Loading(loading.Value);
    }

    public static Button HandleClick(this Button button, Func<Event<Button>, ValueTask> onClick)
    {
        return button with { OnClick = onClick };
    }

    public static Button HandleClick(this Button button, Action<Event<Button>> onClick)
    {
        return button with { OnClick = onClick.ToValueTask() };
    }

    public static Button HandleClick(this Button button, Action onClick)
    {
        return button with { OnClick = _ => { onClick(); return ValueTask.CompletedTask; } };
    }

    public static Button HandleClick(this Button button, Func<ValueTask> onClick)
    {
        return button with { OnClick = _ => onClick() };
    }

    public static Button Tag(this Button button, object tag)
    {
        return button with { Tag = tag };
    }

    public static Button Content(this Button button, object child)
    {
        return button with { Children = [child] };
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Primary(this Button button)
    {
        return button.Variant(ButtonVariant.Primary);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Secondary(this Button button)
    {
        return button.Variant(ButtonVariant.Secondary);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Outline(this Button button)
    {
        return button.Variant(ButtonVariant.Outline);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Destructive(this Button button)
    {
        return button.Variant(ButtonVariant.Destructive);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Ghost(this Button button)
    {
        return button.Variant(ButtonVariant.Ghost);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Link(this Button button)
    {
        return button.Variant(ButtonVariant.Link);
    }

    [RelatedTo(nameof(Button.Variant))]
    public static Button Inline(this Button button)
    {
        return button.Variant(ButtonVariant.Inline);
    }

    public static Button BorderRadius(this Button button, BorderRadius radius)
    {
        return button with { BorderRadius = radius };
    }

    public static Button Size(this Button button, Sizes size)
    {
        return button with { Size = size };
    }

    [RelatedTo(nameof(Button.Size))]
    public static Button Large(this Button button)
    {
        return button.Size(Sizes.Large);
    }

    [RelatedTo(nameof(Button.Size))]
    public static Button Small(this Button button)
    {
        return button.Size(Sizes.Small);
    }
}
