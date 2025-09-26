namespace Ivy.Core.Hooks;

/// <summary>
/// Interface providing access to all hooks functionality including state management, effects, context, and services.
/// This is the main entry point for using hooks within views and components.
/// </summary>
public interface IViewContext : IDisposable
{
    /// <summary>
    /// Tracks a disposable resource to be cleaned up when the view context is disposed.
    /// </summary>
    /// <param name="disposable">The disposable resource to track.</param>
    void TrackDisposable(IDisposable disposable);

    /// <summary>
    /// Tracks multiple disposable resources to be cleaned up when the view context is disposed.
    /// </summary>
    /// <param name="disposables">The disposable resources to track.</param>
    void TrackDisposable(IEnumerable<IDisposable> disposables);

    /// <summary>
    /// Resets the hook calling index to prepare for a new render cycle.
    /// </summary>
    void Reset();

    /// <summary>
    /// Creates or retrieves reactive state with the specified initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="initialValue">The initial value for the state.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
    IState<T> UseState<T>(T? initialValue = default, bool buildOnChange = true);

    /// <summary>
    /// Creates or retrieves reactive state with a factory function for the initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="buildInitialValue">Factory function to create the initial value.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
    IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true);

    /// <summary>
    /// Registers an async effect with optional triggers.
    /// </summary>
    /// <param name="handler">The async effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers);

    /// <summary>
    /// Registers an async effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The async effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers);

    /// <summary>
    /// Registers a synchronous effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The sync effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers);

    /// <summary>
    /// Registers a simple synchronous effect.
    /// </summary>
    /// <param name="handler">The sync effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers);

    /// <summary>
    /// Creates or retrieves a context object using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the context object.</typeparam>
    /// <param name="factory">Factory function to create the context if it doesn't exist.</param>
    /// <returns>The context object.</returns>
    T CreateContext<T>(Func<T> factory);

    /// <summary>
    /// Retrieves a context object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the context to retrieve.</typeparam>
    /// <returns>The context object.</returns>
    T UseContext<T>();

    /// <summary>
    /// Retrieves a context object of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of the context to retrieve.</param>
    /// <returns>The context object.</returns>
    object UseContext(Type serviceType);

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    T UseService<T>();

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <param name="serviceType">The type of the service to retrieve.</param>
    /// <returns>The service instance.</returns>
    object UseService(Type serviceType);

    /// <summary>
    /// Retrieves application arguments of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of arguments to retrieve.</typeparam>
    /// <returns>The arguments object, or null if not found.</returns>
    T? UseArgs<T>() where T : class;
}