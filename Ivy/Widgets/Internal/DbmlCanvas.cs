using Ivy.Core;

namespace Ivy.Widgets.Internal;

/// <summary>
/// Represents a DBML (Database Markup Language) canvas widget that renders database schema diagrams from DBML code.
/// </summary>
public record DbmlCanvas : WidgetBase<DbmlCanvas>
{
    /// <summary>
    /// Initializes a new instance of the DbmlCanvas class with the specified DBML code.
    /// </summary>
    /// <param name="dbml">The DBML code string to parse and visualize. Can be null for an empty canvas.</param>
    public DbmlCanvas(string? dbml)
    {
        Dbml = dbml;
    }

    /// <summary>
    /// Gets or sets the DBML code string that defines the database schema to visualize.
    /// This property contains the Database Markup Language syntax that describes tables, fields, relationships, and database structure.
    /// When set, the widget automatically parses the DBML and renders the corresponding visual diagram.
    /// </summary>
    [Prop] public string? Dbml { get; set; }
}
