using System.Collections.Concurrent;
using Ivy.Core;
using Ivy.Core.Helpers;

namespace Ivy.Apps;

public class AppSession : IDisposable
{
    private readonly Disposables _disposables = new();
    private bool _isDisposed = false;

    public void TrackDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public required string ConnectionId { get; set; }

    public required string AppId { get; set; }

    public required string MachineId { get; set; }

    public required string? ParentId { get; set; }

    public required IWidgetTree WidgetTree { get; set; }

    public required AppDescriptor AppDescriptor { get; set; }

    public required ViewBase App { get; set; }

    public required IContentBuilder ContentBuilder { get; set; }

    public required IServiceProvider AppServices { get; set; }

    public required DateTime LastInteraction { get; set; }

    internal ConcurrentDictionary<Type, object> Signals { get; set; } = new();

    public void Dispose()
    {
        _isDisposed = true;
        _disposables.Dispose();
        WidgetTree.Dispose();
    }

    public bool IsDisposed() => _isDisposed;
}