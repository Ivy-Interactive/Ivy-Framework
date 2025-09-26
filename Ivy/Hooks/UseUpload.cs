using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseUploadExtensions
{
    public static IState<string?> UseUpload<TView>(this TView view, Action<byte[]> handler, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseUpload(handler, mimeType, fileName);

    public static IState<string?> UseUpload<TView>(this TView view, Func<byte[], Task> handler, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseUpload(handler, mimeType, fileName);

    public static IState<string?> UseUpload(this IViewContext context, Action<byte[]> handler, string mimeType, string fileName) =>
        context.UseUpload(bytes => { handler(bytes); return Task.CompletedTask; }, mimeType, fileName);

    public static IState<string?> UseUpload(this IViewContext context, Func<byte[], Task> handler, string mimeType, string fileName)
    {
        var url = context.UseState<string?>();
        var uploadService = context.UseService<IUploadService>();
        context.UseEffect(() =>
        {
            var (cleanup, uploadUrl) = uploadService.AddUpload(handler, mimeType, fileName);
            url.Set(uploadUrl);
            return cleanup;
        });
        return url;
    }
}