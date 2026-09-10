// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// WebMCP hooks, mirrored onto <see cref="ViewBase"/> so they can be called bare inside
/// <c>Build()</c>. The implementations are extension methods on <see cref="IViewContext"/> in
/// <c>Hooks/UseWebMcpTool.cs</c>; <see cref="ViewBase"/> does not implement that interface, so
/// without these mirrors a view would have to write <c>this.Context.UseWebMcpTool(...)</c>.
/// </summary>
public abstract partial class ViewBase
{
    /// <inheritdoc cref="UseWebMcpToolExtensions.UseWebMcpAvailability" />
    protected IState<WebMcpAvailability> UseWebMcpAvailability() =>
        this.Context.UseWebMcpAvailability();

    protected void UseWebMcpTool(string name, string description, Action handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool(string name, string description, Func<object?> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool(string name, string description, Func<Task> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool(string name, string description, Func<Task<object?>> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool<TArgs>(string name, string description, Action<TArgs> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool<TArgs>(string name, string description, Func<TArgs, object?> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool<TArgs>(string name, string description, Func<TArgs, Task> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);

    protected void UseWebMcpTool<TArgs>(string name, string description, Func<TArgs, Task<object?>> handler, WebMcpToolOptions? options = null) =>
        this.Context.UseWebMcpTool(name, description, handler, options);
}
