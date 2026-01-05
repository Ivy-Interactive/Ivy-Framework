using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Views.Forms;
using System.Runtime.CompilerServices;
using static Ivy.Views.Forms.FormHelpers;

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

    internal Sheet() { }

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

    [OverloadResolutionPriority(-1)]
    public static IView ToSheet(this object content, IState<bool> isOpen, string? title = null, string? description = null, Size? width = null)
    {
        return new FuncView(_ =>
        {
            if (!isOpen.Value) return null;

            return new Sheet(_ =>
            {
                isOpen.Value = false;
                return ValueTask.CompletedTask;
            }, content, title, description).Width(width ?? Sheet.DefaultWidth);
        });
    }

    [OverloadResolutionPriority(1)]
    public static IView ToSheet<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
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
                    .HandleClick(_ => HandleSubmitAndClose())
                    .Scale(formBuilder._scale)
                | new Button("Cancel").Variant(ButtonVariant.Outline).HandleClick(_ => isOpen.Set(false))
                    .Scale(formBuilder._scale)
                | validationView,
                formView
            );

            return new Sheet(_ =>
            {
                isOpen.Value = false;
            }, layout, title, description).Width(width ?? Sheet.DefaultWidth);
        });
    }
}

public class WithSheetView(Button trigger, Func<object> contentFactory, string? title, string? description, Size? width) : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
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
