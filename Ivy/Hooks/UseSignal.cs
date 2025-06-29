using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reflection;
using Ivy.Apps;
using Ivy.Core.Hooks;

namespace Ivy.Hooks;

public struct Unit { }

public interface ISignalSender<TInput, TOutput>
{
    public Task<TOutput[]> Send(TInput input);
}

public interface ISignalReceiver<out TInput, in TOutput>
{
    public IDisposable Receive(Func<TInput, TOutput> callback);
}

public abstract class AbstractSignal<TInput, TOutput>() : ISignalSender<TInput, TOutput>
{
    private readonly ConcurrentDictionary<Guid, Func<TInput, TOutput>> _subscribers = new();

    public async Task<TOutput[]> Send(TInput input)
    {
        var tasks = _subscribers.Values.Select(async callback =>
        {
            try
            {
                return await Task.Run(() => callback(input));
            }
            catch (Exception)
            {
                return default;
            }
        });
        return (await Task.WhenAll(tasks))!;
    }

    public IDisposable Receive(Guid receiverId, Func<TInput, TOutput> callback)
    {
        _subscribers.TryRemove(receiverId, out _);
        _subscribers.TryAdd(receiverId, callback);
        return Disposable.Create(() =>
        {
            _subscribers.TryRemove(receiverId, out _);
        });
    }
}

public class SignalReceiver<TInput, TOutput>(Guid receiverId, AbstractSignal<TInput, TOutput> signal) : ISignalReceiver<TInput, TOutput>
{
    public IDisposable Receive(Func<TInput, TOutput> callback)
    {
        return signal.Receive(receiverId, callback);
    }
}

public static class UseSignalExtensions
{
    public static ISignalSender<TInput, TOutput> CreateSignal<T, TInput, TOutput>(this IViewContext context) where T : AbstractSignal<TInput, TOutput>
    {
        var signalType = typeof(T);
        if (signalType.GetBroadcastType() is { } broadcastType)
        {
            var signalHub = context.UseService<SignalRouter>();
            var appArgs = context.UseService<AppArgs>();
            return signalHub.CreateSignal<T, TInput, TOutput>(signalType, broadcastType, appArgs.ConnectionId);
        }
        return context.CreateContext(Activator.CreateInstance<T>);
    }

    public static ISignalReceiver<TInput, TOutput> UseSignal<T, TInput, TOutput>(this IViewContext view) where T : AbstractSignal<TInput, TOutput>
    {
        var receiverId = view.UseState(Guid.NewGuid, buildOnChange: false);
        var signalType = typeof(T);
        if (signalType.GetBroadcastType() is not null)
        {
            var signalHub = view.UseService<SignalRouter>();
            var appArgs = view.UseService<AppArgs>();
            return signalHub.UseSignal<T, TInput, TOutput>(signalType, receiverId.Value, appArgs.ConnectionId);
        }
        var signal = view.UseContext<T>();
        return new SignalReceiver<TInput, TOutput>(receiverId.Value, signal);
    }
}

public enum BroadcastType
{
    Server,
    Machine,
    App,
    Chrome
}

public class SignalAttribute(BroadcastType broadcastTypeType) : Attribute
{
    public BroadcastType BroadcastType { get; } = broadcastTypeType;
}

public static class HubSignalExtensions
{
    public static BroadcastType? GetBroadcastType(this Type type)
        => type.GetCustomAttribute<SignalAttribute>()?.BroadcastType;
}

public class SignalRouter(AppSessionStore sessionStore)
{
    public ISignalSender<TInput, TOutput> CreateSignal<T, TInput, TOutput>(
        Type signalType,
        BroadcastType broadcastType,
        string connectionId
    ) where T : AbstractSignal<TInput, TOutput>
    {
        return new RouterSignalSender<TInput, TOutput, T>(signalType, broadcastType, connectionId, sessionStore);
    }

    public ISignalReceiver<TInput, TOutput> UseSignal<T, TInput, TOutput>(
        Type signalType,
        Guid receiverId,
        string connectionId
    ) where T : AbstractSignal<TInput, TOutput>
    {
        var session = GetSession(connectionId);
        var signal = (T)GetOrAddSignal(signalType, session);
        return new SignalReceiver<TInput, TOutput>(receiverId, signal);
    }

    private AppSession GetSession(string connectionId)
    {
        return sessionStore.Sessions.TryGetValue(connectionId, out var session) ? session
            : throw new InvalidOperationException("Session not found.");
    }

    private static object GetOrAddSignal(Type signalType, AppSession session)
    {
        return session.Signals.GetOrAdd(signalType, _ => Activator.CreateInstance(signalType)!);
    }

    private class RouterSignalSender<TInput, TOutput, TSignal>(
        Type signalType,
        BroadcastType broadcastType,
        string connectionId,
        AppSessionStore store
    ) : ISignalSender<TInput, TOutput>
        where TSignal : AbstractSignal<TInput, TOutput>
    {
        public async Task<TOutput[]> Send(TInput input)
        {
            var session = store.Sessions[connectionId];
            var sessions = GetTargetSessions(broadcastType, session, store);
            var signals = sessions.Select(s => (TSignal)GetOrAddSignal(signalType, s));
            var tasks = signals.Select(signal => signal.Send(input));
            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r).ToArray();
        }

        private static List<AppSession> GetTargetSessions(BroadcastType type, AppSession session, AppSessionStore store)
        {
            return type switch
            {
                BroadcastType.Server =>
                    store.Sessions.Values.Where(s => !s.IsDisposed()).ToList(),
                BroadcastType.Machine =>
                    store.Sessions.Values.Where(s => !s.IsDisposed() && s.MachineId == store.Sessions[session.ConnectionId].MachineId).ToList(),
                BroadcastType.App =>
                    store.Sessions.Values.Where(s => !s.IsDisposed() && s.AppId == store.Sessions[session.ConnectionId].AppId).ToList(),
                BroadcastType.Chrome =>
                    store.FindChrome(session) is { } chrome ? [chrome] : [],
                _ => []
            };
        }
    }
}

