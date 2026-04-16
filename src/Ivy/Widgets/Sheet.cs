using Ivy.Core;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum SheetSide
{
    Left,
    Right,
    Top,
    Bottom
}

/// <summary>
/// A side-aligned modal panel for editing or displaying distinct sub-tasks.
/// </summary>
public record Sheet : WidgetBase<Sheet>
{
    public static Size DefaultWidth => Size.Rem(24);
    public static Size DefaultHeight => Size.Rem(16);

    [OverloadResolutionPriority(1)]
    public Sheet(Func<Event<Sheet>, ValueTask>? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose.ToEventHandler();
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    // Overload for Action<Event<Sheet>>
    public Sheet(Action<Event<Sheet>>? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose.ToEventHandler();
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    // Overload for simple Action (no parameters)
    public Sheet(Action? onClose, object content, string? title = null, string? description = null) : base([new Slot("Content", content)])
    {
        OnClose = onClose == null ? null : new(_ => { onClose(); return ValueTask.CompletedTask; });
        Title = title;
        Description = description;
        Width = DefaultWidth;
    }

    internal Sheet() { }

    [Prop] public string? Title { get; }

    [Prop] public string? Description { get; }

    [Prop] public SheetSide Side { get; init; } = SheetSide.Right;

    [Prop] public bool Resizable { get; init; } = false;

    [Event] public EventHandler<Event<Sheet>>? OnClose { get; set; }

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
    public static Sheet Side(this Sheet sheet, SheetSide side) => sheet with { Side = side };

    /// <summary>
    /// Enables drag-to-resize on the inner edge of the sheet. Users can drag to adjust the sheet width/height at runtime.
    /// Use .Width(Size.Rem(24).Min(Size.Px(200)).Max(Size.Px(800))) to customize constraints.
    /// Default constraints when resizable: 200px min and 80vw max.
    /// </summary>
    public static Sheet Resizable(this Sheet sheet, bool resizable = true)
    {
        if (!resizable)
        {
            return sheet with { Resizable = false };
        }

        var isHorizontal = sheet.Side is SheetSide.Left or SheetSide.Right;

        if (isHorizontal)
        {
            var width = sheet.Width?.Default ?? Sheet.DefaultWidth;
            if (width.Min == null)
            {
                width = width.Min(Size.Px(200));
            }
            if (width.Max == null)
            {
                width = width.Max(Size.Px(1200));
            }
            return (sheet with { Resizable = true }).Width(width);
        }
        else
        {
            var height = sheet.Height?.Default ?? Sheet.DefaultHeight;
            if (height.Min == null)
            {
                height = height.Min(Size.Px(100));
            }
            if (height.Max == null)
            {
                height = height.Max(Size.Px(900));
            }
            return (sheet with { Resizable = true }).Height(height);
        }
    }

    public static IView WithSheet(this Button trigger, Func<object> contentFactory, string? title = null, string? description = null, Size? width = null, SheetSide side = SheetSide.Right, bool resizable = false)
    {
        return new WithSheetView(trigger, contentFactory, title, description, width, side, resizable);
    }

    public static IView WithSheet(this Button trigger, Func<Action, object> contentFactory, string? title = null, string? description = null, Size? width = null, SheetSide side = SheetSide.Right, bool resizable = false)
    {
        return new WithSheetViewWithClose(trigger, contentFactory, title, description, width, side, resizable);
    }

    [OverloadResolutionPriority(-1)]
    public static IView ToSheet(this object content, IState<bool> isOpen, string? title = null, string? description = null, Size? width = null, SheetSide side = SheetSide.Right)
    {
        return new FuncView(_ =>
        {
            if (!isOpen.Value) return null;

            var sheet = new Sheet(_ =>
            {
                isOpen.Value = false;
                return ValueTask.CompletedTask;
            }, content, title, description) with
            { Side = side };

            // Use Height for top/bottom, Width for left/right
            if (side is SheetSide.Top or SheetSide.Bottom)
            {
                return sheet.Height(width ?? Sheet.DefaultHeight);
            }
            return sheet.Width(width ?? Sheet.DefaultWidth);
        });
    }

    [OverloadResolutionPriority(1)]
    public static IView ToSheet<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null, SheetSide side = SheetSide.Right)
    {
        return new FuncView((context) =>
        {
            (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) =
                formBuilder.UseForm(context);

            var (handleSubmit, isUploading) = context.UseUploadAwareSubmit(formBuilder.GetModel(), onSubmit);

            if (!isOpen.Value) return null; //shouldn't happen

            async ValueTask HandleSubmitAndClose()
            {
                if (await handleSubmit())
                {
                    isOpen.Value = false;
                }
            }

            var isLoading = loading || isUploading;

            var layout = new FooterLayout(
                Layout.Horizontal().Gap(2)
                | FormBuilder<TModel>.DefaultSubmitBuilder(submitTitle ?? "Save")(isLoading)
                    .OnClick(_ => HandleSubmitAndClose())
                    .Density(formBuilder._density)
                | new Button("Cancel").Variant(ButtonVariant.Outline).OnClick(_ => isOpen.Set(false))
                    .Density(formBuilder._density)
                | validationView,
                formView
            );

            var sheet = new Sheet(_ =>
            {
                isOpen.Value = false;
            }, layout, title, description) with
            { Side = side };

            // Use Height for top/bottom, Width for left/right
            if (side is SheetSide.Top or SheetSide.Bottom)
            {
                return sheet.Height(width ?? Sheet.DefaultHeight);
            }
            return sheet.Width(width ?? Sheet.DefaultWidth);
        });
    }
}

public class WithSheetView(Button trigger, Func<object> contentFactory, string? title, string? description, Size? width, SheetSide side = SheetSide.Right, bool resizable = false) : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var clonedTrigger = trigger with
        {
            OnClick = new(_ =>
            {
                isOpen.Value = true;
                return ValueTask.CompletedTask;
            })
        };

        Sheet? sheet = null;
        if (isOpen.Value)
        {
            sheet = new Sheet(_ =>
            {
                isOpen.Value = false;
                return ValueTask.CompletedTask;
            }, contentFactory(), title, description) with
            { Side = side };

            // Use Height for top/bottom, Width for left/right
            if (side is SheetSide.Top or SheetSide.Bottom)
            {
                sheet = sheet.Height(width ?? Sheet.DefaultHeight);
            }
            else
            {
                sheet = sheet.Width(width ?? Sheet.DefaultWidth);
            }

            if (resizable)
            {
                sheet = sheet.Resizable();
            }
        }

        return new Fragment(clonedTrigger, sheet);
    }
}

public class WithSheetViewWithClose(Button trigger, Func<Action, object> contentFactory, string? title, string? description, Size? width, SheetSide side = SheetSide.Right, bool resizable = false) : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var clonedTrigger = trigger with
        {
            OnClick = new(_ =>
            {
                isOpen.Value = true;
                return ValueTask.CompletedTask;
            })
        };

        Sheet? sheet = null;
        if (isOpen.Value)
        {
            var close = () => { isOpen.Value = false; };
            sheet = new Sheet(_ =>
            {
                isOpen.Value = false;
                return ValueTask.CompletedTask;
            }, contentFactory(close), title, description) with
            { Side = side };

            // Use Height for top/bottom, Width for left/right
            if (side is SheetSide.Top or SheetSide.Bottom)
            {
                sheet = sheet.Height(width ?? Sheet.DefaultHeight);
            }
            else
            {
                sheet = sheet.Width(width ?? Sheet.DefaultWidth);
            }

            if (resizable)
            {
                sheet = sheet.Resizable();
            }
        }

        return new Fragment(clonedTrigger, sheet);
    }
}
