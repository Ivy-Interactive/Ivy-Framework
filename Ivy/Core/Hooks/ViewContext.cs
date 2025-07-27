using System.ComponentModel.Design;
using System.Reactive.Disposables;
using Ivy.Apps;
using Ivy.Core.Helpers;

namespace Ivy.Core.Hooks;

public class ViewContext : IViewContext
{
    private readonly Action _requestRefresh;
    private readonly IViewContext? _ancestorContext;
    private readonly IServiceProvider _appServices;
    private readonly Disposables _disposables = new();
    private readonly Dictionary<int, StateHook> _hooks = new();
    private readonly Dictionary<int, EffectHook> _effects = new();
    private readonly EffectQueue _effectQueue;
    private readonly IServiceContainer _services;
    private readonly HashSet<Type> _registeredService;
    private int _callingIndex = 0;

    public ViewContext(Action requestRefresh, IViewContext? ancestorContext, IServiceProvider appServices)
    {
        _requestRefresh = requestRefresh;
        _ancestorContext = ancestorContext;
        _effectQueue = new EffectQueue();
        _disposables.Add(_effectQueue);
        _appServices = appServices;

        var services = new ServiceContainer();
        _registeredService = new HashSet<Type>();
        _disposables.Add(services);
        _services = services;
    }

    public void Refresh()
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
                    return Disposable.Empty;
                },
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers)
    {
        UseEffectHook(
            EffectHook.Create(
                _callingIndex++,
                async () => await handler(),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers)
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
                    return Disposable.Empty;
                }),
                triggers.Select(e => e.ToTrigger()).ToArray()
            )
        );
    }

    public T CreateContext<T>(Func<T?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var type = typeof(T);

        if (_registeredService.Contains(type))
        {
            return (T)_services.GetService(type)!;
        }

        T context = factory()!;
        _services.AddService(type, context);
        _registeredService.Add(type);

        if (context is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return context;
    }

    public T UseService<T>()
    {
        if (_appServices.GetService(typeof(T)) is T service)
        {
            return service;
        }
        return default!;
    }

    public object UseService(Type serviceType)
    {
        if (_appServices.GetService(serviceType) is { } globalService)
        {
            return globalService;
        }
        throw new InvalidOperationException("Context not found.");
    }

    public T? UseArgs<T>() where T : class
    {
        var args = UseService<AppArgs>();
        return args.GetArgs<T>();
    }

    public T UseContext<T>()
    {
        if (_services.GetService(typeof(T)) is T existingService)
        {
            return existingService;
        }

        if (_ancestorContext == null)
        {
            throw new InvalidOperationException($"Context '{typeof(T).FullName}' not found.");
        }

        var service = _ancestorContext.UseContext<T>();

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

        if (_ancestorContext == null)
        {
            throw new InvalidOperationException($"Context '{serviceType.FullName}' not found.");
        }

        var service = _ancestorContext.UseContext(serviceType);

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
                    _effectQueue.Enqueue(effect, EffectPriority.AfterRender);
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
                        trigger.State?.SubscribeAny(() => _effectQueue.Enqueue(effect, EffectPriority.StateChange)) ?? Disposable.Empty
                    );
                    break;
                case EffectTriggerType.AfterRender:
                    _effectQueue.Enqueue(effect, EffectPriority.AfterRender);
                    break;
                case EffectTriggerType.AfterInit:
                    _effectQueue.Enqueue(effect, EffectPriority.AfterInit);
                    break;
            }
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _hooks.Clear();
        _effects.Clear();
    }
}