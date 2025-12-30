using System.Reactive.Disposables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Ivy.Helpers;

namespace Ivy.Hooks;

public class DownloadController(AppSessionStore sessionStore, Server server) : Controller
{
    [Route("ivy/download/{connectionId}/{downloadId}")]
    public async Task<IActionResult> Download(string connectionId, string downloadId)
    {
        if (!sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            throw new Exception($"Download 'ivy/download/{connectionId}/{downloadId}' not found.");
        }

        if (await this.ValidateAuthIfRequired(server, session.AppServices) is { } errorResult)
        {
            return errorResult;
        }

        var downloadService = session.AppServices.GetRequiredService<IDownloadService>();
        return await downloadService.Download(downloadId);
    }
}

public class DownloadService(string connectionId) : IDownloadService, IDisposable
{
    private readonly Dictionary<Guid, (Func<Task<byte[]>> factory, string mimeType, string fileName)> _downloads = new();

    public (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType, string fileName)
    {
        var downloadId = Guid.NewGuid();
        _downloads[downloadId] = (factory, mimeType, fileName);

        var cleanup = Disposable.Create(() =>
        {
            _downloads.Remove(downloadId);
        });

        return (cleanup, $"/ivy/download/{connectionId}/{downloadId}");
    }

    public async Task<IActionResult> Download(string downloadId)
    {
        if (!_downloads.TryGetValue(Guid.Parse(downloadId), out var download))
        {
            throw new Exception($"Download '{downloadId}' not found.");
        }

        var (factory, contentType, fileName) = download;
        return new FileContentResult(await factory(), contentType) { FileDownloadName = fileName };
    }

    public void Dispose()
    {
    }
}

public interface IDownloadService
{
    (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType, string fileName);

    Task<IActionResult> Download(string downloadId);
}