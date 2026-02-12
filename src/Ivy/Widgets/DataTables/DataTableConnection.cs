// ReSharper disable once CheckNamespace
namespace Ivy;

public record DataTableConnection(int Port, string Path, string ConnectionId, string SourceId)
{
    private IClientNotifier? _clientNotifier;

    internal void SetClientNotifier(IClientNotifier notifier)
    {
        _clientNotifier = notifier;
    }

    internal async Task NotifyChange()
    {
        if (_clientNotifier != null)
        {
            await _clientNotifier.NotifyClientAsync(ConnectionId, "DataTableRefresh", SourceId);
        }
    }
}