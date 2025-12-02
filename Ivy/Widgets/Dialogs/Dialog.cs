using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Dialog : WidgetBase<Dialog>
{
    public static Size DefaultWidth => Size.Rem(24);

    [OverloadResolutionPriority(1)]
    public Dialog(Func<Event<Dialog>, ValueTask> onClose, DialogHeader header, DialogBody body, DialogFooter footer) : base([header, body, footer])
    {
        OnClose = onClose;
    }

    [Event] public Func<Event<Dialog>, ValueTask> OnClose { get; set; }

    public static Dialog operator |(Dialog dialog, object child)
    {
        throw new NotSupportedException("Dialog does not support children.");
    }

    public Dialog(Action<Event<Dialog>> onClose, DialogHeader header, DialogBody body, DialogFooter footer)
    : this(e => { onClose(e); return ValueTask.CompletedTask; }, header, body, footer)
    {
    }
}
