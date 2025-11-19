using Ivy.Core.Hooks;

namespace Ivy.Core;

public interface IView : IDisposable
{
    public IViewContext Context { get; }

    public string? Id { get; set; }

    public string? Key { get; set; }

    public object? Build();

    void BeforeBuild(IViewContext context);

    void AfterBuild();

    public bool IsStateless { get; }

    public void TrackDisposable(params IDisposable[] disposable);
}