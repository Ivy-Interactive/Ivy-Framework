using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Core;

/// <summary>
/// Abstract base class for all views in the Ivy framework, providing React-like component functionality with hooks support.
/// Views are C# classes that implement server-side rendering with reactive state management and lifecycle methods.
/// </summary>
public abstract class ViewBase() : IView, IViewContextOwner
{
    /// <summary>
    /// Initializes a new instance of the ViewBase class with the specified key.
    /// </summary>
    /// <param name="key">The optional key for this view, used for efficient re-rendering and identification.</param>
    protected ViewBase(string? key) : this()
    {
        Key = key;
    }

    private IViewContext? _context = null;

    private readonly Disposables _disposables = new();

    /// <summary>
    /// Gets the view context that provides access to hooks like UseState, UseEffect, and services.
    /// Only accessible during the Build method execution.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed outside the Build method or on stateless views.</exception>
    public IViewContext Context
    {
        get
        {
            if (_context == null)
            {
                throw new InvalidOperationException("Access to Context is only allowed in the Build method. Also make sure the view is not IStateless.");
            }
            return _context;
        }
    }

    private string? _id;
    /// <summary>
    /// Gets or sets the unique identifier for this view instance within the widget tree.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access an uninitialized Id.</exception>
    public string? Id
    {
        get
        {
            if (_id == null)
            {
                throw new InvalidOperationException($"Trying to access an uninitialized ViewBase Id in a {this.GetType().FullName} view.");
            }
            return _id;
        }
        set => _id = value;
    }

    /// <summary>Gets or sets the optional key for this view, used for efficient re-rendering and component identification.</summary>
    public string? Key { get; set; }

    /// <summary>
    /// Abstract method that must be implemented by derived classes to define the view's visual content.
    /// Called during each render cycle and should return widgets, other views, or data objects.
    /// </summary>
    /// <returns>The visual content as widgets, views, or other displayable objects.</returns>
    public abstract object? Build();

    /// <summary>
    /// Called before the Build method to initialize the view context and prepare for rendering.
    /// </summary>
    /// <param name="context">The view context providing access to hooks and services.</param>
    public void BeforeBuild(IViewContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Called after the Build method to clean up the view context and finalize the render cycle.
    /// </summary>
    public void AfterBuild()
    {
        _context = null!;
    }

    /// <summary>Gets whether this view is stateless and should not have access to hooks or context.</summary>
    public bool IsStateless => this is IStateless;

    /// <summary>
    /// Tracks disposable resources to be automatically cleaned up when the view is disposed.
    /// </summary>
    /// <param name="disposables">The disposable resources to track for cleanup.</param>
    public void TrackDisposable(params IDisposable[] disposables)
    {
        _disposables.Add(disposables);
    }

    /// <summary>
    /// Tracks multiple disposable resources to be automatically cleaned up when the view is disposed.
    /// </summary>
    /// <param name="disposables">The disposable resources to track for cleanup.</param>
    public void TrackDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables.Add(disposables);
    }

    /// <summary>
    /// Disposes the view and all tracked disposable resources.
    /// </summary>
    public void Dispose()
    {
        (_disposables as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Creates or retrieves a context object using the provided factory function.
    /// </summary>
    /// <typeparam name="T">The type of the context object.</typeparam>
    /// <param name="factory">Factory function to create the context if it doesn't exist.</param>
    protected void CreateContext<T>(Func<T> factory) =>
        this.Context.CreateContext(factory);

    /// <summary>
    /// Retrieves a context object of the specified type from the context hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of the context to retrieve.</typeparam>
    /// <returns>The context object.</returns>
    protected T UseContext<T>()
        => this.Context.UseContext<T>();

    /// <summary>
    /// Retrieves a context object of the specified type from the context hierarchy.
    /// </summary>
    /// <param name="type">The type of the context to retrieve.</param>
    /// <returns>The context object.</returns>
    protected object UseContext(Type type)
        => this.Context.UseContext(type);

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    protected T UseService<T>()
        => this.Context.UseService<T>();

    /// <summary>
    /// Retrieves a service from the application service provider.
    /// </summary>
    /// <param name="type">The type of the service to retrieve.</param>
    /// <returns>The service instance.</returns>
    protected object UseService(Type type)
        => this.Context.UseService(type);

    /// <summary>
    /// Creates or retrieves reactive state with the specified initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="initialValue">The initial value for the state.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
    protected IState<T> UseState<T>(T? initialValue = default(T?), bool buildOnChange = true) =>
        this.Context.UseState(initialValue!, buildOnChange);

    /// <summary>
    /// Creates or retrieves reactive state with a factory function for the initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="buildInitialValue">Factory function to create the initial value.</param>
    /// <param name="buildOnChange">Whether changes to this state should trigger re-rendering.</param>
    /// <returns>A reactive state object.</returns>
    protected IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true) =>
        this.Context.UseState(buildInitialValue, buildOnChange);

    /// <summary>
    /// Registers an async effect with optional triggers.
    /// </summary>
    /// <param name="handler">The async effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    [OverloadResolutionPriority(1)]
    protected void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    /// <summary>
    /// Registers an async effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The async effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    protected void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    /// <summary>
    /// Registers a synchronous effect that returns a cleanup disposable.
    /// </summary>
    /// <param name="handler">The sync effect handler that returns cleanup.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    protected void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    /// <summary>
    /// Registers a simple synchronous effect.
    /// </summary>
    /// <param name="handler">The sync effect handler.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    protected void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    /// <summary>
    /// Retrieves application arguments of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of arguments to retrieve.</typeparam>
    /// <returns>The arguments object, or null if not found.</returns>
    protected T? UseArgs<T>() where T : class =>
        this.Context.UseArgs<T>();

}

