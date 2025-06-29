using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Hooks;

public static class UseDownloadExtensions
{
    //todo: add support for Streams, Strings (urls) etc. Also maybe we should cache the factory result. 

    public static IState<string?> UseDownload<TView>(this TView view, Func<byte[]> factory, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseDownload(() => Task.FromResult(factory()), mimeType, fileName);

    public static IState<string?> UseDownload<TView>(this TView view, Func<Task<byte[]>> factory, string mimeType, string fileName) where TView : ViewBase =>
        view.Context.UseDownload(factory, mimeType, fileName);

    public static IState<string?> UseDownload(this IViewContext context, Func<Task<byte[]>> factory, string mimeType, string fileName)
    {
        var url = context.UseState<string?>();
        var downloadService = context.UseService<IDownloadService>();
        context.UseEffect(() =>
        {
            var (cleanup, downloadUrl) = downloadService.AddDownload(factory, mimeType, fileName);
            url.Set(downloadUrl);
            return cleanup;
        });
        return url;
    }
}