using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract partial class ViewBase() : IView, IViewContextOwner
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

    public string? Id
    {
        get
        {
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Trying to access an uninitialized ViewBase Id in a {this.GetType().FullName} view.");
            }

            return field;
        }
        set;
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
}
