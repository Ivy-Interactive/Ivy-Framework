using Microsoft.AspNetCore.SignalR;

namespace Ivy;

public interface IClientNotifier
{
    Task NotifyClientAsync(string connectionId, string method, object? message);
}

public class ClientNotifier(IHubContext<AppHub> hubContext) : IClientNotifier
{
    public async Task NotifyClientAsync(string connectionId, string method, object? message)
    {
        try
        {
            await hubContext.Clients.Client(connectionId).SendAsync(method, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Failed to notify client {connectionId} with method {method}: {ex.Message}");
        }
    }
}