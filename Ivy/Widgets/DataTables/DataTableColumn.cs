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
/// </summary>
public static class DataTableIcons
{
    public const string User = "User";
    public const string Mail = "Mail";
    public const string Hash = "Hash";
    public const string Calendar = "Calendar";
    public const string Clock = "Clock";
    public const string Activity = "Activity";
    public const string Flag = "Flag";
    public const string Zap = "Zap";
    public const string Info = "Info";
    public const string ChevronUp = "ChevronUp";
    public const string ChevronDown = "ChevronDown";
    public const string Filter = "Filter";
    public const string Search = "Search";
    public const string Settings = "Settings";
    public const string MoreVertical = "MoreVertical";
    public const string HelpCircle = "HelpCircle";
}

