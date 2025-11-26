using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Sheet : WidgetBase<Sheet>
{
    public static Size DefaultWidth => Size.Rem(24);

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

    public static Sheet operator |(Sheet widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }

        return widget with { Children = [new Slot("Content", child)] };
    }
}

public static class SheetExtensions
{
    public static IView WithSheet(this Button trigger, Func<object> contentFactory, string? title = null, string? description = null, Size? width = null)
    {
        return new WithSheetView(trigger, contentFactory, title, description, width);
    }
}

public class WithSheetView(Button trigger, Func<object> contentFactory, string? title, string? description, Size? width) : ViewBase
{
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
