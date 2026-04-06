using Ivy.Core;
using static Ivy.FormHelpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseFormExtensions
{
    public static (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm<TModel>(this IViewContext context, Func<FormBuilder<TModel>> factory)
    {
        return context.UseState(factory, buildOnChange: false).Value.UseForm(context);
    }

    /// <summary>
    /// Creates upload-aware form submission handling with toast notifications for in-progress uploads.
    /// </summary>
    public static (Func<ValueTask<bool>> handleSubmit, bool isUploading) UseUploadAwareSubmit<TModel>(
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
}
