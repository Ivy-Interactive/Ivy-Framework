using System.Reactive;
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
    Ghost,
    Link,
    Inline
}

public record Button : WidgetBase<Button>
{
    public Button(string? title = null, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick;
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
    [Event] public Action<Event<Button>>? OnClick { get; set; }

    public object? Tag { get; set; } //not a prop!

    public static Button operator |(Button widget, object child)
    {
        throw new NotSupportedException("Button does not support children.");
    }
}

public static class ButtonExtensions
{
    public static Button ToButton(this Icons icon, Action<Event<Button>>? onClick = null, ButtonVariant variant = ButtonVariant.Primary)
    {
        return new Button(null, onClick, icon: icon, variant: variant);
    }

    public static IView ToTrigger(this Button trigger, Func<IState<bool>, object> action)
    {
        return new FuncView((context) =>
            {
                var isOpen = context.UseState(false);
                var clonedTrigger = trigger with
                {
                    OnClick = @event =>
                {
                    trigger.OnClick?.Invoke(@event);
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
        return button with { Url = url };
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

    public static Button HandleClick(this Button button, Action<Event<Button>> onClick)
    {
        return button with { OnClick = onClick };
    }

    public static Button HandleClick(this Button button, Action onClick)
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
