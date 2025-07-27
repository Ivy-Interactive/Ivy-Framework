using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Forms;

public static class UseFormExtensions
{
    public static (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(this IViewContext context, Func<FormBuilder<TModel>> factory)
    {
        return context.UseState(factory, buildOnChange: false).Value.UseForm(context);
    }

    public static IView ToSheet<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
    {
        return new FuncView((context) =>
            {
                (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) = formBuilder.UseForm(context);

                if (!isOpen.Value) return null; //shouldn't happen

                async void HandleSubmit()
                {
                    if (await onSubmit())
                    {
                        isOpen.Value = false;
                    }
                }

                var layout = new FooterLayout(
                    Layout.Horizontal().Gap(2)
                        | new Button(submitTitle ?? formBuilder.SubmitTitle).HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                            .Loading(loading).Disabled(loading)
                        | new Button("Cancel").Variant(ButtonVariant.Outline).HandleClick(_ => isOpen.Set(false))
                        | validationView,
                    formView
                );

                return new Sheet(_ =>
                {
                    isOpen.Value = false;
                }, layout, title, description).Width(width ?? Sheet.DefaultWidth);
            }
        );
    }

    public static IView ToDialog<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
    {
        return new FuncView((context) =>
            {
                (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) = formBuilder.UseForm(context);

                if (!isOpen.Value) return null; //shouldn't happen

                async void HandleSubmit()
                {
                    if (await onSubmit())
                    {
                        isOpen.Value = false;
                    }
                }

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
                        new Button("Cancel", _ => isOpen.Value = false, variant: ButtonVariant.Outline),
                        new Button(submitTitle ?? formBuilder.SubmitTitle).HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                            .Loading(loading).Disabled(loading)
                    )
                ).Width(width ?? Dialog.DefaultWidth);
            }
        );
    }
}
