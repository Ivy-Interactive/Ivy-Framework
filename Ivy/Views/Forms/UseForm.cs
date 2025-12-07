using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using static Ivy.Views.Forms.FormHelpers;

namespace Ivy.Views.Forms;

public static class UseFormExtensions
{
    public static (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(this IViewContext context, Func<FormBuilder<TModel>> factory)
    {
        return context.UseState(factory, buildOnChange: false).Value.UseForm(context);
    }

    /// <summary>
    /// Creates upload-aware form submission handling with toast notifications for in-progress uploads.
    /// </summary>
    internal static (Func<ValueTask<bool>> handleSubmit, bool isUploading) UseUploadAwareSubmit<TModel>(
        this IViewContext context,
        IState<TModel> model,
        Func<Task<bool>> onSubmit)
    {
        var hasUploading = context.UseState(false);
        var client = context.UseService<IClientProvider>();

        context.UseEffect(() =>
        {
            hasUploading.Set(CheckForLoadingUploads(model.Value));
        }, model);

        async ValueTask<bool> HandleSubmit()
        {
            if (hasUploading.Value)
            {
                client.Toast(
                    "File uploads are still in progress. Please wait for them to complete.",
                    "Uploads in Progress"
                );
                return false;
            }
            return await onSubmit();
        }

        return (HandleSubmit, hasUploading.Value);
    }

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
            }
        );
    }

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
                            .HandleClick(_ => HandleSubmitAndClose())
                            .Scale(formBuilder._scale)
                    )
                ).Width(width ?? Dialog.DefaultWidth);
            }
        );
    }
}
