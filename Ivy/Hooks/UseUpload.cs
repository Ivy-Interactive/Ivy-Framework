using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseUploadExtensions
{
    public static IState<string?> UseUpload<TView>(this TView view, Action<FileUpload> handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload<TView>(this TView view, Func<FileUpload, Task> handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload<TView>(this TView view, Func<FileUpload, Task<IDisposable?>> handler, string? defaultContentType = null, string? defaultFileName = null) where TView : ViewBase =>
        view.Context.UseUpload(handler, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload(this IViewContext context, Action<FileUpload> handler, string? defaultContentType = null, string? defaultFileName = null) =>
        context.UseUpload(upload => { handler(upload); return Task.FromResult<IDisposable?>(null); }, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload(this IViewContext context, Func<FileUpload, Task> handler, string? defaultContentType = null, string? defaultFileName = null) =>
        context.UseUpload(async upload => { await handler(upload); return null; }, defaultContentType, defaultFileName);

    public static IState<string?> UseUpload(this IViewContext context, Func<FileUpload, Task<IDisposable?>> handler, string? defaultContentType = null, string? defaultFileName = null)
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
        context.UseUpload(bytes => { handler(bytes); return Task.FromResult<IDisposable?>(null); }, mimeType, fileName);

    public static IState<string?> UseUpload(this IViewContext context, Func<byte[], Task> handler, string mimeType, string fileName)
    {
        // Adapt byte[] handler to FileUpload handler
        async Task<IDisposable?> AdaptedHandler(FileUpload fileUpload)
        {
            using var memoryStream = new MemoryStream();
            await fileUpload.Stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            await handler(bytes);
            return null;
        }

        return context.UseUpload((Func<FileUpload, Task<IDisposable?>>)AdaptedHandler, mimeType, fileName);
    }
}