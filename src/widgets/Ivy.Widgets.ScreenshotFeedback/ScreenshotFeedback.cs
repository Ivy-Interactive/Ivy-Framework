using System.Text.Json.Serialization;
using Ivy;
using Ivy.Core;

namespace Ivy.Widgets.ScreenshotFeedback;

[ExternalWidget("frontend/dist/Ivy_Widgets_ScreenshotFeedback.js",
    ExportName = "ScreenshotFeedback",
    GlobalName = "Ivy_Widgets_ScreenshotFeedback",
    StylePath = "frontend/dist/ivy-widgets-screenshot-feedback.css")]
public record ScreenshotFeedback : WidgetBase<ScreenshotFeedback>
{
    public ScreenshotFeedback() { }

    [Prop] public string? UploadUrl { get; init; }

    [Prop] public bool IsOpen { get; init; }

    [Event] public Func<Event<ScreenshotFeedback, AnnotationData>, ValueTask>? OnSave { get; init; }

    [Event] public Func<Event<ScreenshotFeedback>, ValueTask>? OnCancel { get; init; }
}

public record AnnotationData
{
    public AnnotationShape[] Shapes { get; init; } = [];
    public int ScreenshotWidth { get; init; }
    public int ScreenshotHeight { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tool")]
[JsonDerivedType(typeof(CalloutAnnotation), "callout")]
[JsonDerivedType(typeof(FreehandAnnotation), "freehand")]
[JsonDerivedType(typeof(ArrowAnnotation), "arrow")]
[JsonDerivedType(typeof(LineAnnotation), "line")]
[JsonDerivedType(typeof(RectangleAnnotation), "rectangle")]
[JsonDerivedType(typeof(CircleAnnotation), "circle")]
[JsonDerivedType(typeof(CensorAnnotation), "censor")]
[JsonDerivedType(typeof(TextAnnotation), "text")]
public record AnnotationShape
{
    public string Color { get; init; } = "";
    public int LineWidth { get; init; }
}

public record AnnotationPoint
{
    public double X { get; init; }
    public double Y { get; init; }
}

public record CalloutAnnotation : AnnotationShape
{
    public AnnotationPoint Anchor { get; init; } = new();
    public AnnotationPoint Label { get; init; } = new();
    public int Number { get; init; }
    public string Text { get; init; } = "";
}

public record FreehandAnnotation : AnnotationShape
{
    public AnnotationPoint[] Points { get; init; } = [];
}

public record ArrowAnnotation : AnnotationShape
{
    public AnnotationPoint Start { get; init; } = new();
    public AnnotationPoint End { get; init; } = new();
}

public record LineAnnotation : AnnotationShape
{
    public AnnotationPoint Start { get; init; } = new();
    public AnnotationPoint End { get; init; } = new();
}

public record RectangleAnnotation : AnnotationShape
{
    public AnnotationPoint Start { get; init; } = new();
    public AnnotationPoint End { get; init; } = new();
}

public record CircleAnnotation : AnnotationShape
{
    public AnnotationPoint Center { get; init; } = new();
    public double Radius { get; init; }
}

public record CensorAnnotation : AnnotationShape
{
    public AnnotationPoint Start { get; init; } = new();
    public AnnotationPoint End { get; init; } = new();
}

public record TextAnnotation : AnnotationShape
{
    public AnnotationPoint Position { get; init; } = new();
    public string Text { get; init; } = "";
    public double FontSize { get; init; }
}
