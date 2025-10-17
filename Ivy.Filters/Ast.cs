namespace Ivy.Filters;

/// <summary>
/// Base class for all AST nodes in the advanced filter filter
/// </summary>
public abstract record Node;

/// <summary>
/// Logical AND operation between two nodes
/// </summary>
public record And(Node Left, Node Right) : Node;

/// <summary>
/// Logical OR operation between two nodes
/// </summary>
public record Or(Node Left, Node Right) : Node;

/// <summary>
/// Logical NOT operation (negation) of a node
/// </summary>
public record Not(Node Inner) : Node;

/// <summary>
/// Normalized operator types for filter operations
/// </summary>
public enum Op
{
    // Text operations
    Contains,
    NotContains,
    StartsWith,
    EndsWith,

    // Comparison operations (works for numbers, dates, etc.)
    Equals,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // Existence operations
    Blank,
    NotBlank
}

/// <summary>
/// Column data types supported by the filter system
/// </summary>
public enum FieldType
{
    Text,
    Number,
    Date,
    DateTime,
    Boolean
}

/// <summary>
/// Metadata about a column including display name, internal ID, and data type
/// </summary>
public record FieldMeta(string DisplayName, string ColId, FieldType Type)
{
    /// <summary>
    /// Constructor for creating FieldMeta from a PropertyInfo
    /// </summary>
    public FieldMeta(string name, Type propertyType)
        : this(name, name, MapToFieldType(propertyType))
    {
    }

    private static FieldType MapToFieldType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return FieldType.Text;
        if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
            underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
            underlyingType == typeof(float))
            return FieldType.Number;
        if (underlyingType == typeof(DateTime))
            return FieldType.DateTime;
        if (underlyingType == typeof(DateOnly))
            return FieldType.Date;
        if (underlyingType == typeof(bool))
            return FieldType.Boolean;

        return FieldType.Text; // Default
    }
}

/// <summary>
/// Leaf node representing a single filter condition on a column
/// </summary>
/// <param name="FieldDisplay">The display name of the column as it appears in [brackets]</param>
/// <param name="FieldId">The internal column ID for the grid system</param>
/// <param name="Type">The data type of the column</param>
/// <param name="Op">The normalized operator to apply</param>
/// <param name="A">First operand (or only operand for single-operand operations)</param>
/// <param name="B">Second operand (reserved for future use)</param>
public record Leaf(
    string FieldDisplay,
    string FieldId,
    FieldType Type,
    Op Op,
    object? A = null,
    object? B = null
) : Node;