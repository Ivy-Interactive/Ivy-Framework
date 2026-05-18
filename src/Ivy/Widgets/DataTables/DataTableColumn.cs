using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableColumn
{
    public DataTableColumn()
    {
    }

    public required string Name { get; set; }
    public required string Header { get; set; }

    [JsonPropertyName("type")]
    public required ColType ColType { get; set; }

    public string? Group { get; set; }
    public Size? Width { get; set; }
    public bool Hidden { get; set; } = false;
    public bool Sortable { get; set; } = true;
    public SortDirection SortDirection { get; set; } = SortDirection.None;
    public bool Filterable { get; set; } = true;

    [JsonPropertyName("alignContent")]
    public Align AlignContent { get; set; } = Align.Left;

    public bool WrapText { get; set; } = false;

    public int Order { get; set; } = 0;
    public string? Icon { get; set; } = null;
    public string? Help { get; set; } = null;
    public List<string>? Footer { get; set; } = null;

    public Colors? Color { get; set; }
    public Dictionary<string, string>? BadgeColorMapping { get; set; }
    public NumberFormatStyle? FormatStyle { get; set; } = null;
    public int? Precision { get; set; } = null;
    public string? Currency { get; set; } = null;
    public Func<object, object?>? ValueAccessor { get; set; } = null;

    [JsonIgnore]
    public IDataTableColumnRenderer? Renderer { get; set; } = null;

    /// <summary>
    /// Link type for LinkDisplayRenderer ("url", "email", "phone"). Sent to frontend for URL scheme handling.
    /// </summary>
    public string? LinkType { get; set; } = null;

    /// <summary>
    /// Visual mode for <see cref="AnimatedStatusLabelDisplayRenderer"/> ("Label", "Badge", "SpinnerTimer").
    /// Null for non-AnimatedStatus columns.
    /// </summary>
    public AnimatedStatusMode? AnimatedStatusMode { get; set; } = null;

    /// <summary>
    /// True when this column has a dedicated OnCellAction handler and should show
    /// a visual affordance for clickability in the frontend.
    /// </summary>
    public bool HasCellAction { get; set; } = false;
}

public enum SortDirection
{
    Ascending,
    Descending,
    None
}

public enum ColType
{
    Number,
    Text,
    Boolean,
    Date,
    DateTime,
    Icon,
    Labels,
    Link,
    AnimatedStatus
}

public interface IDataTableColumnRenderer
{
    public bool IsEditable { get; }
    public ColType ColType { get; }
}

public class TextDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Text;
}

public class NumberDisplayRenderer : IDataTableColumnRenderer
{
    public NumberFormatStyle FormatStyle { get; set; } = NumberFormatStyle.Decimal;
    public int Precision { get; set; } = 2;
    public string? Currency { get; set; }
    public bool IsEditable => false;
    public ColType ColType => ColType.Number;
}

public class BoolDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Boolean;
}

public class IconDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Icon;
}

public class ButtonDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Text;
}

public class DateTimeDisplayRenderer : IDataTableColumnRenderer
{
    public string Format { get; set; } = "g"; // General date/time pattern (short time) - should be based on Excel formatting?
    public bool IsEditable => false;
    public ColType ColType => ColType.DateTime;
}

public class ImageDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Text;
}

public class LinkDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public LinkDisplayType Type { get; set; } = LinkDisplayType.Url;
    public ColType ColType => ColType.Link;
}

public enum LinkDisplayType
{
    Url,
    Email,
    Phone,
    [Obsolete("Button type is deprecated. Use explicit button renderers instead.")]
    Button
}

public class ProgressDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Number;
}

public class LabelsDisplayRenderer : IDataTableColumnRenderer
{
    public Colors? Color { get; set; }
    public Dictionary<string, string>? BadgeColorMapping { get; set; }
    public bool IsEditable => false;
    public ColType ColType => ColType.Labels;
}

public enum AnimatedStatusMode
{
    /// <summary>Spinner icon + status text. Text shimmers while running, plain text when done/idle.</summary>
    Label,
    /// <summary>Rounded pill (like Labels) with the text inside. Pill text shimmers while running.</summary>
    Badge,
    /// <summary>Small spinner + plain timer text. The text fades in briefly whenever the cell value changes.</summary>
    SpinnerTimer
}

/// <summary>
/// Renders the cell with one of three animation styles selected by <see cref="Mode"/>:
/// <list type="bullet">
///   <item><see cref="AnimatedStatusMode.Label"/>: spinner + shimmering text while running, plain text when not.</item>
///   <item><see cref="AnimatedStatusMode.Badge"/>: rounded pill whose text shimmers while running, using <see cref="BadgeColorMapping"/> for per-value colors.</item>
///   <item><see cref="AnimatedStatusMode.SpinnerTimer"/>: small spinner + plain text that fades in on value change. Use for elapsed-time cells.</item>
/// </list>
/// The cell value must be encoded as "running:&lt;text&gt;", "done:&lt;text&gt;" or "idle:&lt;text&gt;" — use
/// <see cref="AnimatedStatusValue"/> to build it. An optional right-aligned secondary label can be appended after a tab character.
/// </summary>
public class AnimatedStatusLabelDisplayRenderer : IDataTableColumnRenderer
{
    public AnimatedStatusMode Mode { get; set; } = AnimatedStatusMode.Label;
    public Dictionary<string, string>? BadgeColorMapping { get; set; }
    public bool IsEditable => false;
    public ColType ColType => ColType.AnimatedStatus;
}

/// <summary>
/// Helper for producing the string value expected by <see cref="AnimatedStatusLabelDisplayRenderer"/>.
/// </summary>
public static class AnimatedStatusValue
{
    public static string Running(string statusText, string? rightLabel = null) =>
        Encode("running", statusText, rightLabel);

    public static string Done(string statusText = "Done", string? rightLabel = null) =>
        Encode("done", statusText, rightLabel);

    public static string Idle(string statusText = "-", string? rightLabel = null) =>
        Encode("idle", statusText, rightLabel);

    private static string Encode(string state, string text, string? rightLabel)
    {
        var body = $"{state}:{text ?? string.Empty}";
        return string.IsNullOrEmpty(rightLabel) ? body : $"{body}\t{rightLabel}";
    }
}

