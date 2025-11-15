using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Sheet widget sliding in from screen side to display additional content with customizable widths, titles, descriptions, and close handling.</summary>
public record Sheet : WidgetBase<Sheet>
{
    public static Size DefaultWidth => Size.Rem(24);

    /// <param name="onClose">Event handler called when user closes sheet.</param>
    /// <param name="content">Content to display within sheet.</param>
    /// <param name="title">Optional title text displayed at top of sheet.</param>
    /// <param name="description">Optional description text displayed below title.</param>
    [OverloadResolutionPriority(1)]
    public Sheet(Func<Event<Sheet>, ValueTask>? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose;
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    // Overload for Action<Event<Sheet>>
    public Sheet(Action<Event<Sheet>>? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose?.ToValueTask();
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    // Overload for simple Action (no parameters)
    public Sheet(Action? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose == null ? null : (_ => { onClose(); return ValueTask.CompletedTask; });
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    [Prop] public string? Title { get; }

    [Prop] public string? Description { get; }

    [Event] public Func<Event<Sheet>, ValueTask>? OnClose { get; set; }

    /// <param name="widget">Sheet to add child content to.</param>
    /// <param name="child">Child content to add to sheet.</param>
    /// <returns>New Sheet instance with updated content.</returns>
    /// <exception cref="NotSupportedException">Thrown when adding multiple children at once.</exception>
    public static Sheet operator |(Sheet widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }

        return widget with { Children = [new Slot("Content", child)] };
    }
}

/// <summary>Extension methods for Sheet widget providing fluent API for creating and configuring sheets with common patterns.</summary>
public static class SheetExtensions
{
    /// <param name="trigger">Button that triggers sheet to open when clicked.</param>
    /// <param name="contentFactory">Function creating content to display in sheet.</param>
    /// <param name="title">Optional title text for sheet header.</param>
    /// <param name="description">Optional description text for sheet header.</param>
    /// <param name="width">Optional custom width for sheet. When null, uses default width.</param>
    /// <returns>IView managing trigger button and conditional sheet display.</returns>
    public static IView WithSheet(this Button trigger, Func<object> contentFactory, string? title = null, string? description = null, Size? width = null)
    {
        return new WithSheetView(trigger, contentFactory, title, description, width);
    }
}

/// <summary>Helper view class managing integration between trigger button and sheet with automatic state management and conditional rendering.</summary>
public class WithSheetView(Button trigger, Func<object> contentFactory, string? title, string? description, Size? width) : ViewBase
{
    /// <summary>Builds view integrating trigger button with conditional sheet.</summary>
    /// <returns>Fragment containing trigger button and conditional sheet display.</returns>
    public override object? Build()
    {
        var isOpen = this.UseState(false);
        var clonedTrigger = trigger with
        {
            OnClick = _ =>
            {
                isOpen.Value = true;
                return ValueTask.CompletedTask;
            }
        };
        return new Fragment(
            clonedTrigger,
            isOpen.Value ? new Sheet(_ =>
            {
                isOpen.Value = false;
                return ValueTask.CompletedTask;
            }, contentFactory(), title, description).Width(width ?? Sheet.DefaultWidth) : null
        );
    }
}
