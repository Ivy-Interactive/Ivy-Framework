using System.Reactive.Disposables;
using System.Text.Json;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Core;

public abstract class ViewBase() : IView, IViewContextOwner
{
    protected ViewBase(string? key) : this()
    {
        Key = key;
    }

    private IViewContext? _context = null;

    private readonly Disposables _disposables = new();

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

    public string? Key { get; set; }

    public abstract object? Build();

    public void BeforeBuild(IViewContext context)
    {
        _context = context;
    }

    public void AfterBuild()
    {
        _context = null!;
    }

    public bool IsStateless => this is IStateless;

    public void TrackDisposable(params IDisposable[] disposables)
    {
        _disposables.Add(disposables);
    }

    public void TrackDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables.Add(disposables);
    }

    public void Dispose()
    {
        (_disposables as IDisposable)?.Dispose();
    }

    protected void CreateContext<T>(Func<T> factory) =>
        this.Context.CreateContext(factory);

    protected T UseContext<T>()
        => this.Context.UseContext<T>();

    protected object UseContext(Type type)
        => this.Context.UseContext(type);

    protected T UseService<T>()
        => this.Context.UseService<T>();

    protected object UseService(Type type)
        => this.Context.UseService(type);

    protected IState<T> UseState<T>(T initialValue = default(T), bool buildOnChange = true) =>
        this.Context.UseState(initialValue, buildOnChange);

    protected IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true) =>
        this.Context.UseState(buildInitialValue, buildOnChange);

    protected void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers) =>
        this.Context.UseEffect(handler, triggers);

    protected T? UseArgs<T>() where T : class =>
        this.Context.UseArgs<T>();

}

