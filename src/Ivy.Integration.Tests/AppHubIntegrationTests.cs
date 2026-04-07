using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ivy.Integration.Tests;

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
        connection.On<object?>("Refresh", payload =>
        {
            refreshTcs.TrySetResult(payload);
        });

        await connection.StartAsync();
        await refreshTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Send an event for a non-existent widget — hub should not throw
        await connection.InvokeAsync("Event", "click", "non-existent-widget", (JsonArray?)null);
    }

    [Fact]
    public async Task HubConnection_Disconnect_CleansUpSession()
    {
        var connection = _server.CreateHubConnection();

        var refreshTcs = new TaskCompletionSource<object?>();
        connection.On<object?>("Refresh", payload =>
        {
            refreshTcs.TrySetResult(payload);
        });

        await connection.StartAsync();
        await refreshTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(_server.SessionStore.Sessions.Count > 0, "Session should exist after connect");

        var connectionId = connection.ConnectionId!;
        Assert.True(_server.SessionStore.Sessions.ContainsKey(connectionId), "Session store should contain connection");

        await connection.StopAsync();
        await connection.DisposeAsync();

        // Give server time to process disconnect
        await Task.Delay(500);

        Assert.False(_server.SessionStore.Sessions.ContainsKey(connectionId), "Session should be removed after disconnect");
    }

    [Fact]
    public async Task HubConnection_Reconnect_GetsNewSession()
    {
        // First connection
        await using var connection1 = _server.CreateHubConnection();

        var refreshTcs1 = new TaskCompletionSource<object?>();
        connection1.On<object?>("Refresh", payload =>
        {
            refreshTcs1.TrySetResult(payload);
        });

        await connection1.StartAsync();
        var payload1 = await refreshTcs1.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(payload1);

        await connection1.StopAsync();
        await Task.Delay(500);

        // Second connection
        await using var connection2 = _server.CreateHubConnection();

        var refreshTcs2 = new TaskCompletionSource<object?>();
        connection2.On<object?>("Refresh", payload =>
        {
            refreshTcs2.TrySetResult(payload);
        });

        await connection2.StartAsync();
        var payload2 = await refreshTcs2.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(payload2);

        Assert.Equal(HubConnectionState.Connected, connection2.State);

        // Verify session store has exactly the second connection
        Assert.True(_server.SessionStore.Sessions.ContainsKey(connection2.ConnectionId!));
    }
}
