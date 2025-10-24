using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Forms;

/// <summary>
/// Extension methods for form presentation in modals and custom layouts.
/// </summary>
public static class UseFormExtensions
{
    /// <summary>
    /// Creates form components for custom layouts with cached form builder.
    /// </summary>
    /// <param name="context">View context for state management.</param>
    /// <param name="factory">Factory function to create the form builder.</param>
    /// <returns>Tuple with submit handler, form view, validation view, and loading state.</returns>
    public static (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(this IViewContext context, Func<FormBuilder<TModel>> factory)
    {
        return context.UseState(factory, buildOnChange: false).Value.UseForm(context);
    }

    /// <summary>
    /// Displays the form in a sheet modal with submit/cancel buttons.
    /// </summary>
    /// <param name="formBuilder">The form builder to display.</param>
    /// <param name="isOpen">State controlling sheet visibility.</param>
    /// <param name="title">Optional sheet title.</param>
    /// <param name="description">Optional description text.</param>
    /// <param name="submitTitle">Optional custom submit button text.</param>
    /// <param name="width">Optional sheet width.</param>
    /// <returns>Sheet modal view that closes on successful submission.</returns>
    public static IView ToSheet<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
    {
        return new FuncView((context) =>
            {
                (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) = formBuilder.UseForm(context);

                if (!isOpen.Value) return null; //shouldn't happen

                async ValueTask HandleSubmit()
                {
                    if (await onSubmit())
                    {
                        isOpen.Value = false;
                    }
                }

                var layout = new FooterLayout(
                    Layout.Horizontal().Gap(2)
                        | new Button(submitTitle ?? formBuilder.SubmitTitle).HandleClick(_ => HandleSubmit())
                            .Loading(loading).Disabled(loading).Size(formBuilder.Size)
                        | new Button("Cancel").Variant(ButtonVariant.Outline).HandleClick(_ => isOpen.Set(false))
                            .Size(formBuilder.Size)
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

    /// <summary>
    /// Displays the form in a dialog modal with header, body, and footer structure.
    /// </summary>
    /// <param name="formBuilder">The form builder to display.</param>
    /// <param name="isOpen">State controlling dialog visibility.</param>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="description">Optional description text.</param>
    /// <param name="submitTitle">Optional custom submit button text.</param>
    /// <param name="width">Optional dialog width.</param>
    /// <returns>Dialog modal view that closes on successful submission.</returns>
    public static IView ToDialog<TModel>(this FormBuilder<TModel> formBuilder, IState<bool> isOpen, string? title = null, string? description = null, string? submitTitle = null, Size? width = null)
    {
        return new FuncView((context) =>
            {
                (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) = formBuilder.UseForm(context);

                if (!isOpen.Value) return null; //shouldn't happen

                async ValueTask HandleSubmit()
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
                        new Button("Cancel", _ => isOpen.Value = false, variant: ButtonVariant.Outline).Size(formBuilder.Size),
                        new Button(submitTitle ?? formBuilder.SubmitTitle).HandleClick(_ => HandleSubmit())
                            .Loading(loading).Disabled(loading).Size(formBuilder.Size)
                    )
                ).Width(width ?? Dialog.DefaultWidth);
            }
        );

    }
}
