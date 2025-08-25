using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a sheet widget that slides in from the side of the screen to display
/// additional content while allowing users to dismiss them. Sheets provide a
/// non-intrusive way to show additional information, forms, or complex content
/// without navigating away from the current page.
/// 
/// The Sheet widget creates overlay panels that can contain any content, from
/// simple forms to complex layouts, while maintaining the context of the main
/// interface. Sheets support customizable widths, titles, descriptions, and
/// close event handling for comprehensive user interaction management.
/// </summary>
public record Sheet : WidgetBase<Sheet>
{
    /// <summary>
    /// Gets the default width for sheet widgets when no specific width is specified.
    /// This default provides a balanced size that works well for most sheet content
    /// while maintaining good readability and user experience.
    /// 
    /// The default width is set to 24rem, which provides adequate space for
    /// content while ensuring the sheet doesn't dominate the entire screen.
    /// </summary>
    public static Size DefaultWidth => Size.Rem(24);

    /// <summary>
    /// Initializes a new instance of the Sheet class with the specified close
    /// handler, content, title, and description. The sheet will display the
    /// provided content in an overlay panel with the specified configuration.
    /// </summary>
    /// <param name="onClose">Event handler that is called when the user closes
    /// the sheet. This handler receives the sheet event context and should
    /// perform any necessary cleanup or state management when the sheet is dismissed.</param>
    /// <param name="content">The content to display within the sheet. This can be
    /// any widget, layout, or combination of elements that should be shown
    /// in the sheet overlay.</param>
    /// <param name="title">Optional title text displayed at the top of the sheet.
    /// This provides context about the sheet's purpose and helps users understand
    /// what content they can expect to find within it.</param>
    /// <param name="description">Optional description text displayed below the title.
    /// This provides additional context and explanation about the sheet's content
    /// or purpose, enhancing user understanding.</param>
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

    /// <summary>
    /// Gets the title text displayed at the top of the sheet.
    /// This property provides a clear heading for the sheet content, helping
    /// users understand what information or functionality the sheet contains.
    /// 
    /// The title is typically displayed prominently at the top of the sheet
    /// and should be concise but descriptive of the sheet's purpose.
    /// When null, no title is displayed, creating a cleaner visual appearance.
    /// </summary>
    [Prop] public string? Title { get; }

    /// <summary>
    /// Gets the description text displayed below the title.
    /// This property provides additional context and explanation about the
    /// sheet's content or purpose, helping users better understand what
    /// they can expect from the sheet.
    /// 
    /// The description is typically displayed below the title and can provide
    /// more detailed information about the sheet's functionality or content.
    /// When null, no description is displayed, creating a more compact layout.
    /// </summary>
    [Prop] public string? Description { get; }

    /// <summary>
    /// Gets or sets the event handler that is called when the sheet is closed.
    /// This event handler receives the sheet event context and should perform
    /// any necessary cleanup, state management, or navigation logic when
    /// the user dismisses the sheet.
    /// 
    /// The close event is typically triggered by user actions such as clicking
    /// a close button, pressing the escape key, or clicking outside the sheet.
    /// This handler enables you to manage the sheet's lifecycle and application
    /// state appropriately.
    /// </summary>
    [Event] public Func<Event<Sheet>, ValueTask>? OnClose { get; set; }

    /// <summary>
    /// Operator overload that allows adding a single child to the Sheet using the pipe operator.
    /// This operator enables convenient sheet content construction by allowing you
    /// to chain content elements together for better readability.
    /// 
    /// The operator automatically creates a new Slot for the content and replaces
    /// any existing content, ensuring the sheet maintains its intended structure
    /// while enabling fluent content building. Only single children are supported
    /// to maintain the sheet's focused design.
    /// </summary>
    /// <param name="widget">The Sheet to add the child content to.</param>
    /// <param name="child">The child content to add to the sheet.</param>
    /// <returns>A new Sheet instance with the updated content.</returns>
    /// <exception cref="NotSupportedException">Thrown when attempting to add multiple children at once.</exception>
    public static Sheet operator |(Sheet widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }

        return widget with { Children = [new Slot("Content", child)] };
    }
}

/// <summary>
/// Provides extension methods for the Sheet widget that enable a fluent API for
/// creating and configuring sheets with common patterns. These methods simplify
/// the creation of sheets by providing convenient shortcuts for typical use cases
/// and common configurations.
/// </summary>
public static class SheetExtensions
{
    /// <summary>
    /// Creates a sheet that is triggered by a button click, providing a convenient
    /// way to open sheets from button interactions. This method automatically
    /// handles the sheet's open/close state management and integrates the sheet
    /// with the triggering button.
    /// 
    /// The WithSheet method creates a complete sheet experience with automatic
    /// state management, making it easy to add sheet functionality to buttons
    /// without manual state handling or complex view management.
    /// </summary>
    /// <param name="trigger">The button that will trigger the sheet to open when clicked.</param>
    /// <param name="contentFactory">A function that creates the content to display in the sheet.
    /// This factory function is called when the sheet opens, allowing for dynamic
    /// content creation and ensuring fresh content on each sheet opening.</param>
    /// <param name="title">Optional title text for the sheet header.</param>
    /// <param name="description">Optional description text for the sheet header.</param>
    /// <param name="width">Optional custom width for the sheet. When null, uses the default sheet width.</param>
    /// <returns>An IView that manages the trigger button and conditional sheet display.</returns>
    public static IView WithSheet(this Button trigger, Func<object> contentFactory, string? title = null, string? description = null, Size? width = null)
    {
        return new WithSheetView(trigger, contentFactory, title, description, width);
    }
}

/// <summary>
/// A helper view class that manages the integration between a trigger button
/// and a sheet, handling the open/close state management and conditional
/// rendering of the sheet content.
/// 
/// This class automatically manages the sheet's visibility state, clones the
/// trigger button to add click handling, and conditionally renders the sheet
/// based on the current state. It provides a seamless integration between
/// buttons and sheets without requiring manual state management.
/// </summary>
public class WithSheetView(Button trigger, Func<object> contentFactory, string? title, string? description, Size? width) : ViewBase
{
    /// <summary>
    /// Builds the view that integrates the trigger button with the conditional sheet.
    /// This method creates a fragment containing the modified trigger button and
    /// the conditionally rendered sheet, managing the complete sheet lifecycle.
    /// </summary>
    /// <returns>A Fragment containing the trigger button and conditional sheet display.</returns>
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
