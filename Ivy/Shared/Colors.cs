using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using Utf8JsonReader = System.Text.Json.Utf8JsonReader;
using Utf8JsonWriter = System.Text.Json.Utf8JsonWriter;

namespace Ivy.Shared;

/// <summary>
/// Predefined color palette with light/dark theme support and WCAG accessibility compliance.
/// Includes neutral, chromatic, and semantic colors for consistent UI theming.
/// </summary>
public enum Colors
{
    /// <summary>Pure black color.</summary>
    Black,
    /// <summary>Pure white color.</summary>
    White,
    /// <summary>Cool gray with blue undertones.</summary>
    Slate,
    /// <summary>Neutral gray color.</summary>
    Gray,
    /// <summary>Warm gray with slight brown undertones.</summary>
    Zinc,
    /// <summary>Balanced neutral gray.</summary>
    Neutral,
    /// <summary>Warm gray with beige undertones.</summary>
    Stone,
    /// <summary>Red color.</summary>
    Red,
    /// <summary>Orange color.</summary>
    Orange,
    /// <summary>Amber color.</summary>
    Amber,
    /// <summary>Yellow color.</summary>
    Yellow,
    /// <summary>Lime green color.</summary>
    Lime,
    /// <summary>Green color.</summary>
    Green,
    /// <summary>Emerald green color.</summary>
    Emerald,
    /// <summary>Teal color.</summary>
    Teal,
    /// <summary>Cyan color.</summary>
    Cyan,
    /// <summary>Sky blue color.</summary>
    Sky,
    /// <summary>Blue color.</summary>
    Blue,
    /// <summary>Indigo color.</summary>
    Indigo,
    /// <summary>Violet color.</summary>
    Violet,
    /// <summary>Purple color.</summary>
    Purple,
    /// <summary>Fuchsia color.</summary>
    Fuchsia,
    /// <summary>Pink color.</summary>
    Pink,
    /// <summary>Rose color.</summary>
    Rose,
    /// <summary>Primary theme color for main UI elements.</summary>
    Primary,
    /// <summary>Secondary theme color for supporting UI elements.</summary>
    Secondary,
    /// <summary>Destructive color for dangerous actions and errors.</summary>
    Destructive,
    /// <summary>Success color for positive states and confirmations.</summary>
    Success,
    /// <summary>Warning color for cautionary states and alerts.</summary>
    Warning,
    /// <summary>Info color for informational states and messages.</summary>
    Info,
}
