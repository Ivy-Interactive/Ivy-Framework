using Ivy.Shared;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableColumn
{
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

    [JsonPropertyName("align")]
    public Align Align { get; set; } = Align.Left;

    public int Order { get; set; } = 0;
    public string? Icon { get; set; } = null;
    public string? Help { get; set; } = null;

    [JsonIgnore]
    public IDataTableColumnRenderer? Renderer { get; set; } = null;
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
    Link
}

public interface IDataTableColumnRenderer
{
    public bool IsEditable { get; }
}

public class TextDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class NumberDisplayRenderer : IDataTableColumnRenderer
{
    public string Format { get; set; } = "N2"; // Default format for numbers - should be based on Excel formatting!
    public bool IsEditable => false;
}

public class BoolDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class IconDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class ButtonDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class DateTimeDisplayRenderer : IDataTableColumnRenderer
{
    public string Format { get; set; } = "g"; // General date/time pattern (short time) - should be based on Excel formatting?
    public bool IsEditable => false;
}

public class ImageDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class LinkDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public LinkDisplayType Type { get; set; } = LinkDisplayType.Url;
}

public enum LinkDisplayType
{
    Url,
    Email,
    Phone,
    Button
}

public class ProgressDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

public class LabelsDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

/// <summary>
/// Glide Data Grid compatible icon names for DataTableColumn.
/// These icon names correspond to the icons defined in the frontend headerIcons.ts file.
/// All icons are rendered as SVG paths compatible with Glide Data Grid's SpriteMap format.
/// </summary>
public static class DataTableIcons
{
    /// <summary>User icon - person silhouette</summary>
    public const string User = "User";

    /// <summary>Mail/Email envelope icon</summary>
    public const string Mail = "Mail";

    /// <summary>Hash/Number symbol (#)</summary>
    public const string Hash = "Hash";

    /// <summary>Calendar icon - date picker</summary>
    public const string Calendar = "Calendar";

    /// <summary>Clock icon - time indicator</summary>
    public const string Clock = "Clock";

    /// <summary>Activity icon - line chart/heartbeat</summary>
    public const string Activity = "Activity";

    /// <summary>Flag icon - marker or priority indicator</summary>
    public const string Flag = "Flag";

    /// <summary>Zap/Lightning bolt icon - action or power</summary>
    public const string Zap = "Zap";

    /// <summary>Info icon - information circle</summary>
    public const string Info = "Info";

    /// <summary>ChevronUp - upward pointing arrow</summary>
    public const string ChevronUp = "ChevronUp";

    /// <summary>ChevronDown - downward pointing arrow</summary>
    public const string ChevronDown = "ChevronDown";

    /// <summary>Filter icon - funnel for filtering data</summary>
    public const string Filter = "Filter";

    /// <summary>Search icon - magnifying glass</summary>
    public const string Search = "Search";

    /// <summary>Settings icon - gear or cog</summary>
    public const string Settings = "Settings";

    /// <summary>MoreVertical icon - three vertical dots menu</summary>
    public const string MoreVertical = "MoreVertical";

    /// <summary>HelpCircle icon - question mark in circle</summary>
    public const string HelpCircle = "HelpCircle";
}

