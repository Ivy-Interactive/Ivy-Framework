namespace Ivy.Shared;

/// <summary>
/// Specifies the visual theme mode for the application interface.
/// This controls whether the application uses light, dark, or system-based theming.
/// </summary>
public enum ThemeMode
{
    Light,
    Dark,
    /// <summary>Automatically follows the user's operating system theme preference.</summary>
    System
}