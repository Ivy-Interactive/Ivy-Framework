using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableColumn
{
    public required string Name { get; set; }
    public required string Header { get; set; } 
    public string? Group { get; set; }
    public Size? Width { get; set; }
    public bool Hidden { get; set; } = false;
    public bool Sortable { get; set; } = true;
    public SortDirection SortDirection { get; set; } = SortDirection.None;
    public Align Align { get; set; } = Align.Left;
    public int Order { get; set; } = 0;
    public Icon? Icon { get; set; } = null;
    public string? Help { get; set; } = null;
    public IDataTableColumnRenderer? Renderer { get; set; } = null;
}

public enum SortDirection
{
    Ascending,
    Descending,
    None
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
    public string Format { get; set; } = "N2"; // Default format for numbers - should be based on Excel formatting?
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
    Phone
}

public class ProgressDisplayRenderer : IDataTableColumnRenderer
{
    public bool IsEditable => false;
}

