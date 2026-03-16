using System.Reactive.Disposables;
using Ivy.Core.Auth;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

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
    private readonly Dictionary<Guid, (Func<Task<object>> factory, string mimeType, string fileName, bool isStream)> _downloads = new();

    public (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType, string fileName)
    {
        return AddEntry(async () => (object)await factory(), mimeType, fileName, isStream: false);
    }

    public (IDisposable cleanup, string url) AddStreamDownload(Func<Task<Stream>> factory, string mimeType, string fileName)
    {
        return AddEntry(async () => (object)await factory(), mimeType, fileName, isStream: true);
    }

    private (IDisposable cleanup, string url) AddEntry(Func<Task<object>> factory, string mimeType, string fileName, bool isStream)
    {
        var downloadId = Guid.NewGuid();
        _downloads[downloadId] = (factory, mimeType, fileName, isStream);

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

        var (factory, contentType, fileName, isStream) = download;
        var result = await factory();

        if (isStream)
        {
            var stream = (Stream)result;
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName,
                EnableRangeProcessing = true
            };
        }

        return new FileContentResult((byte[])result, contentType) { FileDownloadName = fileName };
    }

    public void Dispose()
    {
    }
}

public interface IDownloadService
{
    (IDisposable cleanup, string url) AddDownload(Func<Task<byte[]>> factory, string mimeType, string fileName);

    (IDisposable cleanup, string url) AddStreamDownload(Func<Task<Stream>> factory, string mimeType, string fileName);

    Task<IActionResult> Download(string downloadId);
}
