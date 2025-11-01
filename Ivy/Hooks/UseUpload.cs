using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseUploadExtensions
{
    public static IState<UploadContext?> UseUpload<TView>(this TView view, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<UploadContext?> UseUpload(this IViewContext context, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var ctxState = context.UseState<UploadContext?>();
        var uploadService = context.UseService<IUploadService>();
        context.UseEffect(() =>
        {
            var (cleanup, uploadUrl) = uploadService.AddUpload(handler, () => (ctxState.Value?.Accept, ctxState.Value?.MaxFileSize), defaultContentType, defaultFileName);
            ctxState.Set(new UploadContext(uploadUrl, fileId => uploadService.CancelUpload(fileId)));
            return cleanup;
        }, [EffectTrigger.AfterInit()]);
        return ctxState;
    }

    /// <summary>
    /// Creates an upload endpoint using an IUploadHandler for custom upload logic.
    /// </summary>
    /// <param name="view">The view context.</param>
    /// <param name="handler">The upload handler to process uploaded files.</param>
    /// <param name="defaultContentType">Optional default content type for uploaded files.</param>
    /// <param name="defaultFileName">Optional default file name for uploaded files.</param>
    /// <returns>A state containing the upload URL.</returns>
    public static IState<UploadContext?> UseUpload<TView>(this TView view, IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    /// <summary>
    /// Creates an upload endpoint using an IUploadHandler for custom upload logic.
    /// </summary>
    /// <param name="context">The view context.</param>
    /// <param name="handler">The upload handler to process uploaded files.</param>
    /// <param name="defaultContentType">Optional default content type for uploaded files.</param>
    /// <param name="defaultFileName">Optional default file name for uploaded files.</param>
    /// <returns>A state containing the upload URL.</returns>
    public static IState<UploadContext?> UseUpload(this IViewContext context, IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        return context.UseUpload(handler.HandleUploadAsync, defaultContentType, defaultFileName);
    }
}
