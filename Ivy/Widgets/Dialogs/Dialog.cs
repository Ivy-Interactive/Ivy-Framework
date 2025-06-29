using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Dialog : WidgetBase<Dialog>
{
    public static Size DefaultWidth => Size.Rem(24);

    public Dialog(Action<Event<Dialog>> onClose, DialogHeader header, DialogBody body, DialogFooter footer) : base([header, body, footer])
    {
        OnClose = onClose;
    }

    [Event] public Action<Event<Dialog>> OnClose { get; set; }

    public static Dialog operator |(Dialog dialog, object child)
    {
        throw new NotSupportedException("Dialog does not support children.");
    }
}
