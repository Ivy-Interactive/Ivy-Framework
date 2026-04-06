// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseDownloadExtensions
{
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

    public static IState<string?> UseDownload(this IViewContext context, Func<Task<Stream>> factory, string mimeType, string fileName)
    {
        var url = context.UseState<string?>();
        var downloadService = context.UseService<IDownloadService>();
        context.UseEffect(() =>
        {
            var (cleanup, downloadUrl) = downloadService.AddStreamDownload(factory, mimeType, fileName);
            url.Set(downloadUrl);
            return cleanup;
        });
        return url;
    }
}
