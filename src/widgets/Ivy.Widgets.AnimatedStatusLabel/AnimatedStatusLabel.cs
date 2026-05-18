namespace Ivy.Widgets.AnimatedStatusLabel;

[ExternalWidget("frontend/dist/Ivy_Widgets_AnimatedStatusLabel.js",
                StylePath = "frontend/dist/ivy-widgets-animatedstatuslabel.css",
                ExportName = "AnimatedStatusLabel")]
public record AnimatedStatusLabel : WidgetBase<AnimatedStatusLabel>
{
    /// <summary>The status text. Shown with a shimmer animation while not complete.</summary>
    [Prop] public string StatusText { get; init; } = "";

    /// <summary>When true the label switches to a static "done" state (no shimmer).</summary>
    [Prop] public bool IsComplete { get; init; }

    /// <summary>Show the leading icon (spinner while running, check when complete).</summary>
    [Prop] public bool ShowIcon { get; init; } = true;

    /// <summary>Optional right-aligned label (e.g. elapsed time).</summary>
    [Prop] public string? RightLabel { get; init; }

    public AnimatedStatusLabel(string statusText, bool isComplete)
        : base([new Icon(Icons.LoaderCircle).Small(), new Icon(Icons.CircleCheck).Small()])
    {
        StatusText = statusText;
        IsComplete = isComplete;
    }

    public AnimatedStatusLabel(string statusText, bool isComplete, object spinnerIcon, object doneIcon)
        : base([spinnerIcon, doneIcon])
    {
        StatusText = statusText;
        IsComplete = isComplete;
    }
}

public static class AnimatedStatusLabelExtensions
{
    public static AnimatedStatusLabel ShowIcon(this AnimatedStatusLabel w, bool showIcon = true) =>
        w with { ShowIcon = showIcon };

    public static AnimatedStatusLabel RightLabel(this AnimatedStatusLabel w, string? rightLabel) =>
        w with { RightLabel = rightLabel };
}
