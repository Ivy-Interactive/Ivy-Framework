namespace Ivy.Shared;

/// <summary>
/// Specifies the visual theme mode for the application interface.
/// This controls whether the application uses light, dark, or system-based theming.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Light theme with bright backgrounds and dark text.
    /// Best for well-lit environments and users who prefer bright interfaces.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme with dark backgrounds and light text.
    /// Reduces eye strain in low-light environments and saves battery on OLED displays.
    /// </summary>
    Dark,

    /// <summary>
    /// Automatically follows the user's operating system theme preference.
    /// Switches between light and dark themes based on the system setting.
    /// </summary>
    System
}