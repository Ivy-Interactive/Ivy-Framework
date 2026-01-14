using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reflection;
using Ivy.Apps;
using Ivy.Core.Hooks;
using AppContext = Ivy.Apps.AppContext;

namespace Ivy.Hooks;

public struct Unit { }

public interface ISignal<TInput, TOutput>
{
    Task<TOutput[]> Send(TInput input);
    IDisposable Receive(Func<TInput, TOutput> callback);
}

public abstract class AbstractSignal<TInput, TOutput>
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

    internal IDisposable ReceiveWithId(Guid receiverId, Func<TInput, TOutput> callback)
    {
        _subscribers.TryRemove(receiverId, out _);
        _subscribers.TryAdd(receiverId, callback);
        return Disposable.Create(() =>
        {
            _subscribers.TryRemove(receiverId, out _);
        });
    }
}

internal class SignalHandle<TInput, TOutput>(
    AbstractSignal<TInput, TOutput> signal,
    Guid receiverId
) : ISignal<TInput, TOutput>
{
    public Task<TOutput[]> Send(TInput input) => signal.Send(input);

    public IDisposable Receive(Func<TInput, TOutput> callback) => signal.ReceiveWithId(receiverId, callback);
}

public static class UseSignalExtensions
{
    public static ISignal<TInput, TOutput> UseSignal<T, TInput, TOutput>(this IViewContext context) where T : AbstractSignal<TInput, TOutput>
    {
        var receiverId = context.UseRef(Guid.NewGuid);
        var signalType = typeof(T);
        var appContext = context.UseService<AppContext>();
        var sessionStore = context.UseService<AppSessionStore>();
        var session = sessionStore.Sessions[appContext.ConnectionId];
        var signal = (T)session.Signals.GetOrAdd(signalType, _ => Activator.CreateInstance(signalType)!);

        if (signalType.GetBroadcastType() is { } broadcastType)
        {
            var signalHub = context.UseService<SignalRouter>();
            return signalHub.GetSignal<T, TInput, TOutput>(signalType, broadcastType, receiverId.Value, appContext.ConnectionId);
        }

        return new SignalHandle<TInput, TOutput>(signal, receiverId.Value);
    }
}

public enum BroadcastType
{
    Server,
    User,
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
    public ISignal<TInput, TOutput> GetSignal<T, TInput, TOutput>(
        Type signalType,
        BroadcastType broadcastType,
        Guid receiverId,
        string connectionId
    ) where T : AbstractSignal<TInput, TOutput>
    {
        return new RouterSignal<TInput, TOutput, T>(signalType, broadcastType, connectionId, receiverId, sessionStore);
    }

    private static object GetOrAddSignal(Type signalType, AppSession session)
    {
        return session.Signals.GetOrAdd(signalType, _ => Activator.CreateInstance(signalType)!);
    }

    private class RouterSignal<TInput, TOutput, TSignal>(
        Type signalType,
        BroadcastType broadcastType,
        string connectionId,
        Guid receiverId,
        AppSessionStore store
    ) : ISignal<TInput, TOutput>
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

        public IDisposable Receive(Func<TInput, TOutput> callback)
        {
            var session = store.Sessions.TryGetValue(connectionId, out var s) ? s
                : throw new InvalidOperationException("Session not found.");
            var signal = (TSignal)GetOrAddSignal(signalType, session);
            return signal.ReceiveWithId(receiverId, callback);
        }

        private static List<AppSession> GetTargetSessions(BroadcastType type, AppSession session, AppSessionStore store)
        {
            return type switch
            {
                BroadcastType.Server =>
                    store.Sessions.Values.Where(s => !s.IsDisposed()).ToList(),
                BroadcastType.User =>
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

