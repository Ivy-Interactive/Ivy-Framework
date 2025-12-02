using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseUploadExtensions
{
    public static IState<UploadContext> UseUpload<TView>(this TView view, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<UploadContext> UseUpload(this IViewContext context, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var uploadService = context.UseService<IUploadService>();

        // Create a temporary context to get initial values for validation
        var tempContext = new UploadContext("", _ => { });
        var ctxState = context.UseState(tempContext);

        context.UseEffect(() =>
        {
            var (cleanup, uploadUrl) = uploadService.AddUpload(handler, () => (ctxState.Value.Accept, ctxState.Value.MaxFileSize), defaultContentType, defaultFileName);
            ctxState.Set(new UploadContext(uploadUrl, fileId => uploadService.Cancel(fileId))
            {
                Accept = ctxState.Value.Accept,
                MaxFileSize = ctxState.Value.MaxFileSize,
                MaxFiles = ctxState.Value.MaxFiles
            });
            return cleanup;
        }, [EffectTrigger.AfterInit()]);
        return ctxState;
    }

    public static IState<UploadContext> UseUpload<TView>(this TView view, IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
    view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<UploadContext> UseUpload(this IViewContext context, IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        return context.UseUpload(handler.HandleUploadAsync, defaultContentType, defaultFileName);
    }
}
