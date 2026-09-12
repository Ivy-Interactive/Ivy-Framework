using Ivy.Core.Apps;
using Ivy.Core.Server;

namespace Ivy.Test;

[Signal(BroadcastType.AppShell)]
public class TestAppShellSignal : AbstractSignal<string, Unit> { }

[Signal(BroadcastType.User)]
public class TestUserSignal : AbstractSignal<string, Unit> { }

public class SignalRouterTests
{
    private static AppSession CreateSession(string connectionId, string machineId = "machine-1", string appId = "app-1")
    {
        return new AppSession
        {
            ConnectionId = connectionId,
            AppId = appId,
            MachineId = machineId,
            ParentId = null,
            WidgetTree = null!,
            AppDescriptor = null!,
            App = null!,
            ContentBuilder = null!,
            AppServices = null!,
            LastInteraction = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Send_WhenConnectionNotFoundInStore_ReturnsEmptyAndDoesNotThrow()
    {
        using var store = new AppSessionStore();
        var router = new SignalRouter(store);

        var signal = router.GetSignal<TestAppShellSignal, string, Unit>(
            typeof(TestAppShellSignal),
            BroadcastType.AppShell,
            Guid.NewGuid(),
            "missing-connection-id");

        var result = await signal.Send("test-payload");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Send_WhenSessionIsDisposed_ReturnsEmptyAndDoesNotThrow()
    {
        using var store = new AppSessionStore();
        var session = CreateSession("conn-disposed");
        await session.DisposeAsync();
        store.Sessions["conn-disposed"] = session;

        var router = new SignalRouter(store);
        var signal = router.GetSignal<TestAppShellSignal, string, Unit>(
            typeof(TestAppShellSignal),
            BroadcastType.AppShell,
            Guid.NewGuid(),
            "conn-disposed");

        var result = await signal.Send("test-payload");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Receive_WhenConnectionNotFoundInStore_ReturnsDisposableAndDoesNotThrow()
    {
        using var store = new AppSessionStore();
        var router = new SignalRouter(store);

        var signal = router.GetSignal<TestAppShellSignal, string, Unit>(
            typeof(TestAppShellSignal),
            BroadcastType.AppShell,
            Guid.NewGuid(),
            "missing-connection-id");

        var subscription = signal.Receive(_ => default);

        Assert.NotNull(subscription);
        subscription.Dispose(); // Should dispose cleanly without error
    }

    [Fact]
    public async Task Send_BroadcastUser_RoutesOnlyToMatchingMachineId()
    {
        using var store = new AppSessionStore();
        var session1 = CreateSession("conn-1", machineId: "mach-A");
        var session2 = CreateSession("conn-2", machineId: "mach-A");
        var session3 = CreateSession("conn-3", machineId: "mach-B");

        store.Sessions["conn-1"] = session1;
        store.Sessions["conn-2"] = session2;
        store.Sessions["conn-3"] = session3;

        var router = new SignalRouter(store);

        var receivedCount = 0;
        var signal2 = router.GetSignal<TestUserSignal, string, Unit>(
            typeof(TestUserSignal),
            BroadcastType.User,
            Guid.NewGuid(),
            "conn-2");
        signal2.Receive(_ =>
        {
            Interlocked.Increment(ref receivedCount);
            return default;
        });

        var signal3 = router.GetSignal<TestUserSignal, string, Unit>(
            typeof(TestUserSignal),
            BroadcastType.User,
            Guid.NewGuid(),
            "conn-3");
        signal3.Receive(_ =>
        {
            Interlocked.Increment(ref receivedCount);
            return default;
        });

        var senderSignal = router.GetSignal<TestUserSignal, string, Unit>(
            typeof(TestUserSignal),
            BroadcastType.User,
            Guid.NewGuid(),
            "conn-1");

        await senderSignal.Send("hello");

        // Wait brief moment for async Task.Run in AbstractSignal.Send
        await Task.Delay(50);

        // Only session2 has mach-A matching session1
        Assert.Equal(1, receivedCount);
    }
}
