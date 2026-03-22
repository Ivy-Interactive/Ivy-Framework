using System.Reactive;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum BatonVariant
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
    Ai,
}

public enum LinkTarget
{
    Blank,
    Self,
}

/// <summary>
/// An interactive element for triggering actions or navigation.
/// </summary>
public record Baton : WidgetBase<Baton>
{
    internal Baton() { }

    [OverloadResolutionPriority(1)]
    public Baton(string? title = null, Func<Event<Baton>, ValueTask>? onClick = null, BatonVariant variant = BatonVariant.Primary, Icons? icon = null)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick.ToEventHandler();
    }

    public Baton(string? title = null, Action<Event<Baton>>? onClick = null, BatonVariant variant = BatonVariant.Primary, Icons? icon = null)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick.ToEventHandler();
    }

    public Baton(string? title = null, Action? onClick = null, BatonVariant variant = BatonVariant.Primary, Icons? icon = null)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick == null ? null : new(_ => { onClick(); return ValueTask.CompletedTask; });
    }

    public Baton(string? title = null, Func<ValueTask>? onClick = null, BatonVariant variant = BatonVariant.Primary, Icons? icon = null)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick == null ? null : new(_ => onClick());
    }

    [Prop] public string? Title { get; set; }

    [Prop] public BatonVariant Variant { get; set; } = BatonVariant.Primary;

    [Prop] public Icons? Icon { get; set; }

    [Prop] public Align IconPosition { get; set; } = Align.Left;

    [Prop] public Colors? Foreground { get; set; }

    [Prop] public string? Url { get; set; }

    [Prop] public LinkTarget Target { get; set; } = LinkTarget.Self;

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Tooltip { get; set; }

    [Prop] public bool Loading { get; set; }

    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    [Event] public EventHandler<Event<Baton>>? OnClick { get; set; }

    public object? Tag { get; set; } //not a prop!

    public static Baton operator |(Baton widget, object child)
    {
        throw new NotSupportedException("Baton does not support children.");
    }
}

public static class BatonExtensions
{
    [OverloadResolutionPriority(1)]
    public static Baton ToBaton(this Icons icon, Func<Event<Baton>, ValueTask>? onClick = null, BatonVariant variant = BatonVariant.Primary)
    {
        return new Baton(null, onClick, icon: icon, variant: variant);
    }

    public static Baton ToBaton(this Icons icon, Action<Event<Baton>>? onClick = null, BatonVariant variant = BatonVariant.Primary)
    {
        return new Baton(null, onClick, icon: icon, variant: variant);
    }

    public static Baton ToBaton(this Icons icon, Func<ValueTask>? onClick = null, BatonVariant variant = BatonVariant.Primary)
    {
        return new Baton(null, onClick, icon: icon, variant: variant);
    }

    public static IView ToTrigger(this Baton trigger, Func<IState<bool>, object> action)
    {
        return new FuncView((context) =>
            {
                var isOpen = context.UseState(false);
                var clonedTrigger = trigger with
                {
                    OnClick = new(async @event =>
                    {
                        if (trigger.OnClick != null)
                        {
                            await trigger.OnClick.Invoke(@event);
                        }
                        isOpen.Value = true;
                    })
                };
                return new Fragment(
                    clonedTrigger,
                    isOpen.Value ? action(isOpen) : null
                );
            }
        );
    }

    public static Baton Title(this Baton button, string title)
    {
        return button with { Title = title };
    }

    public static Baton Url(this Baton button, string url)
    {
        // Validate URL to prevent open redirect vulnerabilities
        var validatedUrl = ValidationHelper.ValidateLinkUrl(url);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {url}. Only safe URLs (http/https, relative paths, app://, anchors) are allowed.", nameof(url));
        }
        return button with { Url = validatedUrl };
    }

    public static Baton Disabled(this Baton button, bool disabled = true) => button with { Disabled = disabled };

    public static Baton Icon(this Baton button, Icons? icon, Align position = Align.Left) => button with { Icon = icon, IconPosition = position };

    public static Baton Variant(this Baton button, BatonVariant variant) => button with { Variant = variant };

    public static Baton Foreground(this Baton button, Colors color) => button with { Foreground = color };

    public static Baton Tooltip(this Baton button, string tooltip) => button with { Tooltip = tooltip };

    public static Baton Loading(this Baton button, bool loading = true) => button with { Loading = loading };

    public static Baton Loading(this Baton button, IState<bool> loading) => button.Loading(loading.Value);

    public static Baton OnClick(this Baton button, Func<Event<Baton>, ValueTask> onClick) => button with { OnClick = new(onClick) };

    public static Baton OnClick(this Baton button, Action<Event<Baton>> onClick) => button with { OnClick = new(onClick.ToValueTask()) };

    public static Baton OnClick(this Baton button, Action onClick) => button with { OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; }) };

    public static Baton OnClick(this Baton button, Func<ValueTask> onClick) => button with { OnClick = new(_ => onClick()) };

    public static Baton Tag(this Baton button, object tag) => button with { Tag = tag };

    public static Baton Content(this Baton button, object child) => button with { Children = [child] };

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Primary(this Baton button) => button.Variant(BatonVariant.Primary);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Secondary(this Baton button) => button.Variant(BatonVariant.Secondary);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Outline(this Baton button) => button.Variant(BatonVariant.Outline);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Destructive(this Baton button) => button.Variant(BatonVariant.Destructive);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Ghost(this Baton button) => button.Variant(BatonVariant.Ghost);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Link(this Baton button) => button.Variant(BatonVariant.Link);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Inline(this Baton button) => button.Variant(BatonVariant.Inline);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Ai(this Baton button) => button.Variant(BatonVariant.Ai);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Success(this Baton button) => button.Variant(BatonVariant.Success);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Warning(this Baton button) => button.Variant(BatonVariant.Warning);

    [RelatedTo(nameof(Baton.Variant))]
    public static Baton Info(this Baton button) => button.Variant(BatonVariant.Info);

    public static Baton BorderRadius(this Baton button, BorderRadius radius) => button with { BorderRadius = radius };

    [RelatedTo(nameof(Baton.Target))]
    public static Baton Target(this Baton button, LinkTarget target) => button with { Target = target };

    [RelatedTo(nameof(Baton.Target))]
    public static Baton OpenInNewTab(this Baton button, bool openInNewTab = true) => button with { Target = openInNewTab ? LinkTarget.Blank : LinkTarget.Self };
}
