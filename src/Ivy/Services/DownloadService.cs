using System.Reactive.Disposables;
using Ivy.Core.Apps;
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
            return RedirectToErrorApp(
                ErrorAppArgs.ForNotFound("Download not found", "This download link has expired or is invalid."));
        }

        if (await this.ValidateAuthIfRequired(server, session.AppServices) is { } _)
        {
            return RedirectToErrorApp(ErrorAppArgs.ForUnauthorized());
        }

        var downloadService = session.AppServices.GetRequiredService<IDownloadService>();
        try
        {
            return await downloadService.Download(downloadId);
        }
        catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToErrorApp(
                ErrorAppArgs.ForNotFound("Download not found", "The requested file is no longer available.") with { Details = ex.Message });
        }
    }

    private RedirectResult RedirectToErrorApp(ErrorAppArgs args) =>
        Redirect($"/?appId={Uri.EscapeDataString(AppIds.ErrorNotFound)}&appArgs={Uri.EscapeDataString(ErrorAppArgs.ToArgsJson(args))}");
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
