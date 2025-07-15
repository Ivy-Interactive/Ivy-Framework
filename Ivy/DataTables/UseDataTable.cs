using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;

namespace Ivy.DataTables;

public static class UseDataTableExtensions
{
    // public static IState<string?> UseDownload<TView>(this TView view, Func<byte[]> factory, string mimeType, string fileName) where TView : ViewBase =>
    //     view.Context.UseDownload(() => Task.FromResult(factory()), mimeType, fileName);
    //
    // public static IState<string?> UseDownload<TView>(this TView view, Func<Task<byte[]>> factory, string mimeType, string fileName) where TView : ViewBase =>
    //     view.Context.UseDownload(factory, mimeType, fileName);
    //
    // public static IState<string?> UseDownload(this IViewContext context, Func<Task<byte[]>> factory, string mimeType, string fileName)
    // {
    //     var url = context.UseState<string?>();
    //     var downloadService = context.UseService<IDownloadService>();
    //     context.UseEffect(() =>
    //     {
    //         var (cleanup, downloadUrl) = downloadService.AddDownload(factory, mimeType, fileName);
    //         url.Set(downloadUrl);
    //         return cleanup;
    //     });
    //     return url;
    // }
}