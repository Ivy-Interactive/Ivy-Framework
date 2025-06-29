using System.Text.Json.Nodes;

namespace Ivy.Core;

public interface IWidgetTree : IDisposable
{
    public IView RootView { get; }

    public IWidget GetWidgets();

    public Task BuildAsync();

    public void RefreshView(string nodeId);

    public bool TriggerEvent(string widgetId, string eventName, JsonArray args);

    void HotReload();
}