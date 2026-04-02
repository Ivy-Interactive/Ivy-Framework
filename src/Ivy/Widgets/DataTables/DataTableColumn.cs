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

    public int Order { get; set; } = 0;
    public string? Icon { get; set; } = null;
    public string? Help { get; set; } = null;
    public List<string>? Footer { get; set; } = null;

    public NumberFormatStyle? FormatStyle { get; set; } = null;
    public int? Precision { get; set; } = null;
    public string? Currency { get; set; } = null;

    [JsonIgnore]
    public IDataTableColumnRenderer? Renderer { get; set; } = null;

    [JsonIgnore]
    public Func<object, object?>? ValueAccessor { get; set; } = null;
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
    Button
}

public class ProgressDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Number;
}

public class LabelsDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
    public ColType ColType => ColType.Labels;
}

