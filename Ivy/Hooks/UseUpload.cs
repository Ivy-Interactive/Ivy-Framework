using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseUploadExtensions
{
    public static IState<string?> UseUpload<TView>(this TView view, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload(this IViewContext context, UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var url = context.UseState<string?>();
        var uploadService = context.UseService<IUploadService>();
        context.UseEffect(() =>
        {
            var (cleanup, uploadUrl) = uploadService.AddUpload(handler, defaultContentType, defaultFileName);
            url.Set(uploadUrl);
            return cleanup;
        });
        return url;
    }

    public static IState<string?> UseUpload<TView>(this TView view, Action<byte[]> handler, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseUpload(handler, mimeType, fileName);

    public static IState<string?> UseUpload<TView>(this TView view, Func<byte[], Task> handler, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseUpload(handler, mimeType, fileName);

    public static IState<string?> UseUpload(this IViewContext context, Action<byte[]> handler, string mimeType, string fileName) =>
        context.UseUpload(async (bytes) => { handler(bytes); await Task.CompletedTask; }, mimeType, fileName);

    public static IState<string?> UseUpload(this IViewContext context, Func<byte[], Task> handler, string mimeType, string fileName)
    {
        // Adapt byte[] handler to UploadDelegate
        async Task AdaptedHandler(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();
            await handler(bytes);
        }

        return context.UseUpload(AdaptedHandler, mimeType, fileName);
    }
}