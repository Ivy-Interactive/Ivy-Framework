// Resharper disable once CheckNamespace
namespace Ivy;

public interface IClientNotifier
{
    Task NotifyClientAsync(string connectionId, string method, object? message);
}