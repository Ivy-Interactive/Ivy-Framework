using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a modal dialog widget.
/// </summary>
public record Dialog : WidgetBase<Dialog>
{
    /// <summary>
    /// Gets the default width.
    /// </summary>
    public static Size DefaultWidth => Size.Rem(24);

    /// <summary>
    /// Initializes a new instance of the Dialog class.
    /// </summary>
    /// <param name="onClose">
    /// The event handler called when the dialog should be closed.
    /// </param>
    /// <param name="header">
    /// The <see cref="DialogHeader"/> widget.
    /// </param>
    /// <param name="body">
    /// The <see cref="DialogBody"/> widget that contains the main content of the dialog.
    /// </param>
    /// <param name="footer">
    /// The <see cref="DialogFooter"/> widget that typically contains action buttons and controls.
    /// </param>
    [OverloadResolutionPriority(1)]
    public Dialog(Func<Event<Dialog>, ValueTask> onClose, DialogHeader header, DialogBody body, DialogFooter footer) : base([header, body, footer])
    {
        OnClose = onClose;
    }

    /// <summary>
    /// Gets or sets the event handler called when the dialog should be closed.
    /// </summary>
    [Event] public Func<Event<Dialog>, ValueTask> OnClose { get; set; }

    /// <summary>
    /// Overrides the | operator to prevent adding children.
    /// </summary>
    /// <param name="dialog">The Dialog widget.</param>
    /// <param name="child">The child object.</param>
    /// <returns>This method always throws an exception.</returns>
    public static Dialog operator |(Dialog dialog, object child)
    {
        throw new NotSupportedException("Dialog does not support children.");
    }

    /// <summary>
    /// Compatibility constructor.
    /// </summary>
    /// <param name="onClose">The event handler called when the dialog should be closed.</param>
    /// <param name="header">The <see cref="DialogHeader"/> widget.</param>
    /// <param name="body">The <see cref="DialogBody"/> widget.</param>
    /// <param name="footer">The <see cref="DialogFooter"/> widget.</param>
    public Dialog(Action<Event<Dialog>> onClose, DialogHeader header, DialogBody body, DialogFooter footer)
        : this(e => { onClose(e); return ValueTask.CompletedTask; }, header, body, footer)
    {
    }
}
