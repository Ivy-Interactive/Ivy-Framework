using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ivy.Test.Integration;

public class AppHubIntegrationTests : IAsyncLifetime
{
    private IvyTestServer _server = null!;

    public async Task InitializeAsync()
    {
        _server = await IvyTestServer.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        await _server.DisposeAsync();
    }

    [Fact]
    public async Task HubConnection_CanConnect_ReceivesRefresh()
    {
        await using var connection = _server.CreateHubConnection();

        var refreshTcs = new TaskCompletionSource<object?>();
        connection.On<object?>("Refresh", payload =>
        {
            refreshTcs.TrySetResult(payload);
        });

        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        var payload = await refreshTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(payload);

        var json = payload.ToString()!;
        Assert.Contains("widgets", json);
    }

    [Fact]
    public async Task HubConnection_SendEvent_IsProcessed()
    {
        await using var connection = _server.CreateHubConnection();

        var refreshTcs = new TaskCompletionSource<object?>();
        connection.On<object?>("Refresh", payload => refreshTcs.TrySetResult(payload));

        await connection.StartAsync();
        await refreshTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Send an event for a non-existent widget — should not throw
        var exception = await Record.ExceptionAsync(() =>
            connection.InvokeAsync("Event", "click", "nonexistent-widget", (JsonArray?)null));
        Assert.Null(exception);
    }

    [Fact]
    public async Task HubConnection_Disconnect_CleansUpSession()
    {
        var connection = _server.CreateHubConnection();

        var refreshTcs = new TaskCompletionSource<object?>();
        connection.On<object?>("Refresh", payload => refreshTcs.TrySetResult(payload));

        await connection.StartAsync();
        await refreshTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.NotEmpty(_server.SessionStore.Sessions);

        await connection.StopAsync();
        await connection.DisposeAsync();

        // Give server time to process disconnect
        await Task.Delay(500);

        Assert.Empty(_server.SessionStore.Sessions);
    }

    [Fact]
    public async Task HubConnection_Reconnect_GetsNewSession()
    {
        // First connection
        var connection1 = _server.CreateHubConnection();
        var refreshTcs1 = new TaskCompletionSource<object?>();
        connection1.On<object?>("Refresh", payload => refreshTcs1.TrySetResult(payload));

        await connection1.StartAsync();
        var payload1 = await refreshTcs1.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(payload1);

        await connection1.StopAsync();
        await connection1.DisposeAsync();
        await Task.Delay(500);

        // Second connection
        await using var connection2 = _server.CreateHubConnection();
        var refreshTcs2 = new TaskCompletionSource<object?>();
        connection2.On<object?>("Refresh", payload => refreshTcs2.TrySetResult(payload));

        await connection2.StartAsync();
        var payload2 = await refreshTcs2.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(payload2);

        // Verify we got fresh refresh messages for both connections
        Assert.Contains("widgets", payload1.ToString()!);
        Assert.Contains("widgets", payload2.ToString()!);

        // Only second connection should be active
        Assert.Single(_server.SessionStore.Sessions);
    }
}
