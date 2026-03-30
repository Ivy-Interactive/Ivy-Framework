using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Ivy.Core.Exceptions;
using Ivy.Core.Helpers;

namespace Ivy.Core.Hooks;

public class ViewContext : IViewContext
{
    private readonly Action _requestRefresh;
    private readonly IViewContext? _ancestor;
    private readonly IServiceProvider _appServices;
    private readonly Disposables _disposables = new();
    private readonly AsyncDisposables _asyncDisposables = new();
    private readonly Dictionary<int, StateHook> _hooks = new();
    private readonly Dictionary<int, EffectHook> _effects = new();
    private readonly Dictionary<int, ContextHook> _contextHooks = new();
    private readonly EffectQueue _effectQueue;
    private readonly IServiceContainer _services;
    private readonly HashSet<Type> _registeredServices;
    private int _callingIndex;

    public ViewContext(Action requestRefresh, IViewContext? ancestor, IServiceProvider appServices)
    {
        var effectHandler = (appServices.GetService(typeof(IExceptionHandler)) as IExceptionHandler)!;
        _requestRefresh = requestRefresh;
        _ancestor = ancestor;
        _effectQueue = new EffectQueue(effectHandler);
        _asyncDisposables.Add(_effectQueue);
        _appServices = appServices;

        var services = new ServiceContainer();
        _registeredServices = [];
        _disposables.Add(services);
        _services = services;
    }

    private void Refresh()
    {
        _requestRefresh();
    }

    public void TrackDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public void TrackDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables.Add(disposables);
    }

    public void Reset()
    {
        _callingIndex = 0;
    }

    public IState<T> UseState<T>(T? initialValue = default, bool buildOnChange = true) =>
    UseState(() => initialValue!, buildOnChange);

    public IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true)
    {
        if (UseStateHook(
            StateHook.Create(
                _callingIndex++,
                () => new State<T>(buildInitialValue()),
                buildOnChange
            )
        ) is IState<T> typedState)
        {
            return typedState;
        }
        throw new InvalidOperationException("State type mismatch.");
    }

    public void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                async () =>
                {
                    await handler();
                    return null;
                },
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<Task<IDisposable?>> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                async () => new DisposableAdapter(await handler()),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<Task<IAsyncDisposable?>> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                async () => await handler(),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<IDisposable?> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                () => Task.Run(() => (IAsyncDisposable?)new DisposableAdapter(handler())),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<IAsyncDisposable?> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                () => Task.Run(handler),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                () => Task.Run(() =>
                {
                    handler();
                    return (IAsyncDisposable?)null;
                }),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public T CreateContext<T>(Func<T?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var callingIndex = _callingIndex++;
        var type = typeof(T);

        if (_contextHooks.TryGetValue(callingIndex, out var existingHook))
        {
            // Re-evaluate factory to get fresh context with updated state
            T newContext = factory()!;
            existingHook.UpdateInstance(newContext);

            // Update service container so UseContext finds the new instance
            var services = (_services as ServiceContainer)!;
            services.RemoveService(type);
            services.AddService(type, newContext);

            return newContext;
        }

        // First call at this position - create new context hook
        T context = factory()!;
        var hook = new ContextHook(callingIndex, type, context);
        _contextHooks[callingIndex] = hook;

        if (!_registeredServices.Contains(type))
        {
            _services.AddService(type, context);
            _registeredServices.Add(type);
        }
        else
        {
            var services = (_services as ServiceContainer)!;
            services.RemoveService(type);
            services.AddService(type, context);
        }

        if (context is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return context;
    }

    public T UseService<T>()
    {
        if (_appServices.GetService(typeof(T)) is T service)
            return service;
        throw new InvalidOperationException($"Service of type '{typeof(T).FullName}' is not registered.");
    }

    public bool TryUseService<T>([MaybeNullWhen(false)] out T service)
    {
        if (_appServices.GetService(typeof(T)) is T found)
        {
            service = found;
            return true;
        }
        service = default;
        return false;
    }

    public object UseService(Type serviceType)
    {
        if (_appServices.GetService(serviceType) is { } globalService)
        {
            return globalService;
        }
        throw new InvalidOperationException("Context not found.");
    }

    public T UseContext<T>()
    {
        if (_services.GetService(typeof(T)) is T existingService)
        {
            return existingService;
        }

        if (_ancestor == null)
        {
            throw new InvalidOperationException($"Context '{typeof(T).FullName}' not found.");
        }

        var service = _ancestor.UseContext<T>();

        if (service is null)
        {
            throw new InvalidOperationException($"Context '{typeof(T).FullName}' not found.");
        }

        return service;
    }

    public object UseContext(Type serviceType)
    {
        if (_services.GetService(serviceType) is { } existingService)
        {
            return existingService;
        }

        if (_ancestor == null)
        {
            throw new InvalidOperationException($"Context '{serviceType.FullName}' not found.");
        }

        var service = _ancestor.UseContext(serviceType);

        if (service is null)
        {
            throw new InvalidOperationException($"Context '{serviceType.FullName}' not found.");
        }

        return service;
    }

    private IAnyState UseStateHook(StateHook stateHook)
    {
        if (_hooks.TryGetValue(stateHook.Identity, out var existingHook))
        {
            return existingHook.State;
        }

        var state = stateHook.State;
        _hooks[stateHook.Identity] = stateHook;

        _disposables.Add(state);

        if (stateHook.RenderOnChange)
        {
            _disposables.Add(state.SubscribeAny(Refresh));
        }

        return state;
    }

    private void UseEffectHook(EffectHook effect)
    {
        if (!_effects.TryAdd(effect.Identity, effect))
        {
            foreach (var trigger in effect.Triggers)
            {
                if (trigger.Type == EffectTriggerType.AfterRender)
                {
                    _effectQueue.Enqueue(effect, EffectPriority.OnBuild);
                }
            }
            return;
        }

        foreach (var trigger in effect.Triggers)
        {
            switch (trigger.Type)
            {
                case EffectTriggerType.AfterChange:
                    _disposables.Add(
                        trigger.State?.SubscribeAny(() => _effectQueue.Enqueue(effect, EffectPriority.OnStateChange)) ?? Disposable.Empty
                    );
                    break;
                case EffectTriggerType.AfterRender:
                    _effectQueue.Enqueue(effect, EffectPriority.OnBuild);
                    break;
                case EffectTriggerType.AfterInit:
                    _effectQueue.Enqueue(effect, EffectPriority.OnMount);
                    break;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposables.Dispose();
        await _asyncDisposables.DisposeAsync();

        foreach (var hook in _contextHooks.Values)
            hook.Dispose();

        _hooks.Clear();
        _effects.Clear();
        _contextHooks.Clear();
    }
}