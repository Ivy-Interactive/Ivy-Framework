using System.Collections.Concurrent;
using Ivy.Core.Helpers;

namespace Ivy.Core.Apps;

public class AppSession : IAsyncDisposable
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

    public EventDispatchQueue? EventQueue { get; set; }

    // Brokered auth session state
    internal Action<string>? BrokeredTokenAddedHandler { get; set; }
    internal Action<string>? BrokeredTokenRemovedHandler { get; set; }
    internal HashSet<string>? ActiveBrokeredRefreshLoops { get; set; }
    internal ConcurrentDictionary<string, CancellationTokenSource>? BrokeredRefreshCancellations { get; set; }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        EventQueue?.Dispose();
        _disposables.Dispose();
        await WidgetTree.DisposeAsync();
    }

    public bool IsDisposed() => _isDisposed;
}
