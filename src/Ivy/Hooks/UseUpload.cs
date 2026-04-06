// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseUploadExtensions
{
    public static IState<UploadContext> UseUpload(this IViewContext context, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var uploadService = context.UseService<IUploadService>();

        // Create a temporary context to get initial values for validation
        var tempContext = new UploadContext("", _ => { });
        var ctxState = context.UseState(tempContext);

        context.UseEffect(() =>
        {
            var (cleanup, uploadUrl) = uploadService.AddUpload(handler, () => (ctxState.Value.Accept, ctxState.Value.MaxFileSize, ctxState.Value.MinFileSize), defaultContentType, defaultFileName);
            ctxState.Set(ctxState.Value with { UploadUrl = uploadUrl, Cancel = fileId => uploadService.Cancel(fileId) });
            return cleanup;
        }, [EffectTrigger.OnMount()]);
        return ctxState;
    }

    public static IState<UploadContext> UseUpload(this IViewContext context, IUploadHandler handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        return context.UseUpload(handler.HandleUploadAsync, defaultContentType, defaultFileName);
    }
}
