using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Hooks;
using static Ivy.FormHelpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A modal window that interrupts the current workflow to request information or confirmation.
/// </summary>
public record Dialog : WidgetBase<Dialog>
{
    public static Size DefaultWidth => Size.Rem(24);

    [OverloadResolutionPriority(1)]
    public Dialog(Func<Event<Dialog>, ValueTask> onClose, DialogHeader header, DialogBody body, DialogFooter footer) : base([header, body, footer])
    {
        OnClose = new(onClose);
    }

    [Event] public EventHandler<Event<Dialog>>? OnClose { get; set; }

    public static Dialog operator |(Dialog dialog, object child)
    {
        throw new NotSupportedException("Dialog does not support children.");
    }

    public Dialog(Action<Event<Dialog>> onClose, DialogHeader header, DialogBody body, DialogFooter footer)
    : this(onClose.ToValueTask(), header, body, footer)
    {
    }

    internal Dialog() { }
}

public static class DialogExtensions
{
    [OverloadResolutionPriority(-1)]
    public static IView ToDialog(this object content, IState<bool> isOpen, string? title = null, string? description = null, Size? width = null)
    {
        return new FuncView(_ =>
        {
            if (!isOpen.Value) return null;

            return new Dialog(
                _ => isOpen.Set(false),
                new DialogHeader(title ?? ""),
                new DialogBody(
                    Layout.Vertical()
                    | description!
                    | content
                ),
                new DialogFooter()
            ).Width(width ?? Dialog.DefaultWidth);
        });
    }

    [OverloadResolutionPriority(1)]
    public static IView ToDialog<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
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

            return new Dialog(
                _ => isOpen.Set(false),
                new DialogHeader(title ?? ""),
                new DialogBody(
                    Layout.Vertical()
                    | description!
                    | formView
                ),
                new DialogFooter(
                    validationView,
                    new Button("Cancel", _ => isOpen.Value = false, variant: ButtonVariant.Outline).Scale(formBuilder._scale),
                    FormBuilder<TModel>.DefaultSubmitBuilder(submitTitle ?? "Save")(isLoading)
                        .OnClick(_ => HandleSubmitAndClose())
                        .Scale(formBuilder._scale)
                )
            ).Width(width ?? Dialog.DefaultWidth);
        });
    }
}
