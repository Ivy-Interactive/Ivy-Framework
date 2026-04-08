namespace Ivy.Widgets.DiffView;

public enum DiffViewType
{
    Unified,
    Split
}

[ExternalWidget("frontend/dist/Ivy_Widgets_DiffView.js", StylePath = "frontend/dist/ivy-widgets-diffview.css", ExportName = "DiffView")]
public record DiffView : WidgetBase<DiffView>
{
    public DiffView()
    {
        Width = Size.Full();
    }

    /// <summary>The unified diff string (git diff output)</summary>
    [Prop] public string? Diff { get; init; }

    /// <summary>Unified or Split view mode</summary>
    [Prop] public DiffViewType ViewType { get; init; } = DiffViewType.Unified;

    /// <summary>Language hint for syntax highlighting</summary>
    [Prop] public string? Language { get; init; }

    /// <summary>Old file revision name displayed in the header</summary>
    [Prop] public string? OldRevision { get; init; }

    /// <summary>New file revision name displayed in the header</summary>
    [Prop] public string? NewRevision { get; init; }

    /// <summary>Whether to wrap long lines instead of scrolling horizontally</summary>
    [Prop] public bool WordWrap { get; init; } = false;

    [Event] public Func<Event<DiffView, int>, ValueTask>? OnLineClick { get; init; }
}

public static class DiffViewExtensions
{
    public static DiffView Diff(this DiffView w, string diff) => w with { Diff = diff };

    public static DiffView ViewType(this DiffView w, DiffViewType viewType) =>
        w with { ViewType = viewType };

    public static DiffView Split(this DiffView w) => w with { ViewType = DiffViewType.Split };

    public static DiffView Unified(this DiffView w) => w with { ViewType = DiffViewType.Unified };

    public static DiffView Language(this DiffView w, string language) =>
        w with { Language = language };

    public static DiffView OldRevision(this DiffView w, string name) =>
        w with { OldRevision = name };

    public static DiffView NewRevision(this DiffView w, string name) =>
        w with { NewRevision = name };

    public static DiffView WordWrap(this DiffView w, bool wordWrap = true) =>
        w with { WordWrap = wordWrap };

    public static DiffView OnLineClick(
        this DiffView w,
        Func<Event<DiffView, int>, ValueTask> handler
    ) => w with { OnLineClick = handler };

    public static DiffView OnLineClick(this DiffView w, Action<int> handler) =>
        w with
        {
            OnLineClick = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            },
        };
}
