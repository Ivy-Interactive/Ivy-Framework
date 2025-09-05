namespace Ivy.Shared;

/// <summary>
/// Specifies the visual theme mode for the application interface.
/// </summary>
public enum ThemeMode
{
    /// <summary>Light theme with bright backgrounds and dark text.</summary>
    Light,

    /// <summary>Dark theme with dark backgrounds and light text.</summary>
    Dark,

    /// <summary>Automatically follows the user's operating system theme preference.</summary>
    System
}