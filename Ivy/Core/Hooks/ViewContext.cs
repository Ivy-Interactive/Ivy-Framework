using System.ComponentModel.Design;
using System.Reactive.Disposables;
using Ivy.Apps;
using Ivy.Core.Exceptions;
using Ivy.Core.Helpers;

namespace Ivy.Core.Hooks;

/// <summary>
/// Concrete implementation of IViewContext that manages hooks, state, effects, and services for views.
/// Provides hierarchical context support and automatic resource cleanup.
/// </summary>
public class ViewContext : IViewContext
{
    private readonly Action _requestRefresh;
    private readonly IViewContext? _ancestor;
    private readonly IServiceProvider _appServices;
    private readonly Disposables _disposables = new();
    private readonly Dictionary<int, StateHook> _hooks = new();
    private readonly Dictionary<int, EffectHook> _effects = new();
    private readonly EffectQueue _effectQueue;
    private readonly IServiceContainer _services;
    private readonly HashSet<Type> _registeredServices;
    private int _callingIndex;

    /// <summary>
    /// Creates a new view context with the specified refresh callback and optional parent context.
    /// </summary>
    /// <param name="requestRefresh">Callback to trigger view re-rendering.</param>
    /// <param name="ancestor">Optional parent context for hierarchical context lookup.</param>
    /// <param name="appServices">Application service provider for dependency injection.</param>
    public ViewContext(Action requestRefresh, IViewContext? ancestor, IServiceProvider appServices)
    {
        var effectHandler = (appServices.GetService(typeof(IExceptionHandler)) as IExceptionHandler)!;
        _requestRefresh = requestRefresh;
        _ancestor = ancestor;
        _effectQueue = new EffectQueue(effectHandler);
        _disposables.Add(_effectQueue);
        _appServices = appServices;

        var services = new ServiceContainer();
        _registeredServices = [];
        _disposables.Add(services);
        _services = services;
    }

    /// <summary>
    /// Triggers a view refresh by calling the registered refresh callback.
    /// </summary>
    private void Refresh()
    {
        _requestRefresh();
    }

    /// <summary>
    /// Tracks a disposable resource to be cleaned up when the view context is disposed.
    /// </summary>
    /// <param name="disposable">The disposable resource to track.</param>
    public void TrackDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    /// <summary>
    /// Tracks multiple disposable resources to be cleaned up when the view context is disposed.
    /// </summary>
    /// <param name="disposables">The disposable resources to track.</param>
    public void TrackDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables.Add(disposables);
    }

    /// <summary>
    /// Resets the hook calling index to prepare for a new render cycle.
    /// </summary>
    public void Reset()
    {
        _callingIndex = 0;
    }

    /// <summary>
    /// Creates or retrieves reactive state with the specified initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="initialValue">The initial value for the state.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
    public IState<T> UseState<T>(T? initialValue = default, bool buildOnChange = true) =>
        UseState(() => initialValue!, buildOnChange);

    /// <summary>
    /// Creates or retrieves reactive state with a factory function for the initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="buildInitialValue">Factory function to create the initial value.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
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

    /// <summary>
    /// Registers an async effect with optional triggers.
    /// </summary>
    /// <param name="handler">The async effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
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

    /// <summary>
    /// Registers an async effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The async effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
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

    /// <summary>
    /// Registers a synchronous effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The sync effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
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

    /// <summary>
    /// Registers a simple synchronous effect.
    /// </summary>
    /// <param name="handler">The sync effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
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

    /// <summary>
    /// Creates or retrieves a context object using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the context object.</typeparam>
    /// <param name="factory">Factory function to create the context if it doesn't exist.</param>
    /// <returns>The context object.</returns>
    public T CreateContext<T>(Func<T?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var type = typeof(T);

        if (_registeredServices.Contains(type))
        {
            return (T)_services.GetService(type)!;
        }

        T context = factory()!;
        _services.AddService(type, context);
        _registeredServices.Add(type);

        if (context is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return context;
    }

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    public T UseService<T>()
    {
        if (_appServices.GetService(typeof(T)) is T service)
        {
            return service;
        }
        return default!;
    }

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <param name="serviceType">The type of the service to retrieve.</param>
    /// <returns>The service instance.</returns>
    public object UseService(Type serviceType)
    {
        if (_appServices.GetService(serviceType) is { } globalService)
        {
            return globalService;
        }
        throw new InvalidOperationException("Context not found.");
    }

    /// <summary>
    /// Retrieves application arguments of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of arguments to retrieve.</typeparam>
    /// <returns>The arguments object, or null if not found.</returns>
    public T? UseArgs<T>() where T : class
    {
        var args = UseService<AppArgs>();
        return args.GetArgs<T>();
    }

    /// <summary>
    /// Retrieves a context object of the specified type, searching up the context hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of the context to retrieve.</typeparam>
    /// <returns>The context object.</returns>
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

    /// <summary>
    /// Retrieves a context object of the specified type, searching up the context hierarchy.
    /// </summary>
    /// <param name="serviceType">The type of the context to retrieve.</param>
    /// <returns>The context object.</returns>
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

    /// <summary>
    /// Internal method to manage effect hooks, setting up triggers and enqueueing effects for execution.
    /// </summary>
    /// <param name="effect">The effect hook to register.</param>
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

    /// <summary>
    /// Disposes the view context, cleaning up all tracked resources, hooks, and effects.
    /// </summary>
    public void Dispose()
    {
        _disposables.Dispose();
        _hooks.Clear();
        _effects.Clear();
    }
}