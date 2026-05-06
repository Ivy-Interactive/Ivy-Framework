# Theme

Configure the visual appearance of an application including color scheme, light/dark mode, border radius, and scaling. Reflex provides a declarative theme wrapper and a built-in visual theme editor panel. Ivy offers programmatic theming with full custom color schemes and runtime theme switching.

## Reflex

```python
import reflex as rx

app = rx.App(
    theme=rx.theme(
        appearance="dark",
        accent_color="blue",
        gray_color="mauve",
        radius="medium",
        scaling="100%",
        panel_background="solid",
    )
)

# Add a visual theme editor panel
def index():
    return rx.box(
        rx.theme_panel(default_open=True),
    )
```

## Ivy

```csharp
// Server-level theme configuration
var server = new Server()
    .UseTheme(theme => {
        theme.Name = "Ocean";
        theme.Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#0077BE",
                PrimaryForeground = "#FFFFFF",
                Background = "#F0F8FF",
                Foreground = "#1A1A1A",
                Accent = "#87CEEB",
                AccentForeground = "#1A1A1A",
                Muted = "#E0E8F0",
                MutedForeground = "#5A6A7A",
            },
            Dark = new ThemeColors
            {
                Primary = "#4A9EFF",
                PrimaryForeground = "#001122",
                Background = "#001122",
                Foreground = "#E8F4FD",
                Accent = "#1A3A5C",
                AccentForeground = "#E8F4FD",
                Muted = "#0F2A4A",
                MutedForeground = "#8BB3D9",
            }
        };
    });

// Runtime theme mode switching
var client = UseService<IClientProvider>();
client.SetThemeMode(ThemeMode.Dark);

// Runtime custom theme application
var themeService = UseService<IThemeService>();
themeService.SetTheme(customTheme);
var css = themeService.GenerateThemeCss();
client.ApplyTheme(css);
```

## Parameters

| Parameter          | Documentation                                                                 | Ivy                                                                                      |
|--------------------|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `appearance`       | `"inherit"`, `"light"`, `"dark"` - controls light/dark mode                   | `ThemeMode.Light` / `ThemeMode.Dark` / `ThemeMode.System` via `SetThemeMode()`           |
| `accent_color`     | Named color (`"tomato"`, `"red"`, `"blue"`, etc.) - primary accent            | `ThemeColors.Primary` (hex value, e.g. `"#0077BE"`)                                      |
| `gray_color`       | Named gray (`"gray"`, `"mauve"`, `"slate"`, etc.) - neutral tones             | `ThemeColors.Muted` / `ThemeColors.MutedForeground` (hex values)                         |
| `has_background`   | `bool` - whether the theme applies a background                               | Not supported                                                                            |
| `panel_background` | `"solid"`, `"translucent"` - panel background style                           | Not supported                                                                            |
| `radius`           | `"none"`, `"small"`, `"medium"`, `"large"`, `"full"` - global border radius   | Not supported                                                                            |
| `scaling`          | `"90%"`, `"95%"`, `"100%"`, `"105%"`, `"110%"` - global UI scaling            | Not supported                                                                            |
| `theme_panel`      | Built-in visual theme editor (`default_open` bool)                            | Not supported                                                                            |
| —                  | —                                                                             | 21 granular color tokens (Primary, Secondary, Background, Foreground, Border, Card, etc.) |
| —                  | —                                                                             | Separate Light/Dark color schemes via `ThemeColorScheme`                                  |
| —                  | —                                                                             | Runtime theme switching via `IThemeService` + `ApplyTheme()`                              |
