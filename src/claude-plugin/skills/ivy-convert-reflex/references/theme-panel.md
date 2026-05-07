# Theme Panel

A visual editor component for creating and editing application themes at runtime. In Reflex, `rx.theme_panel()` renders an interactive UI panel that lets developers tweak appearance settings (accent color, gray color, radius, scaling, light/dark mode) directly in the browser. Ivy does not have a built-in visual theme panel component but provides programmatic theming through `IThemeService` and `IClientProvider`.

## Reflex

```python
# Add theme panel to your app
rx.theme_panel()

# Open by default
rx.theme_panel(default_open=True)

# Configure the base theme on the app
app = rx.App(
    theme=rx.theme(
        appearance="light",
        has_background=True,
        radius="large",
        accent_color="teal",
    )
)
```

## Ivy

```csharp
// Set theme mode (light/dark/system)
var client = UseService<IClientProvider>();
client.SetThemeMode(ThemeMode.Light);

// Configure a custom theme at the server level
var server = new Server()
    .UseTheme(theme => {
        theme.Name = "Ocean";
        theme.Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#0066cc",
                PrimaryForeground = "#ffffff",
                Background = "#ffffff",
                Foreground = "#111111",
                Accent = "#0099ff",
                AccentForeground = "#ffffff",
                // ... additional color properties
            },
            Dark = new ThemeColors
            {
                Primary = "#3399ff",
                PrimaryForeground = "#ffffff",
                Background = "#1a1a1a",
                Foreground = "#eeeeee",
                Accent = "#66bbff",
                AccentForeground = "#ffffff",
                // ... additional color properties
            }
        };
    });

// Apply theme dynamically at runtime
var themeService = UseService<IThemeService>();
themeService.SetTheme(customTheme);
var css = themeService.GenerateThemeCss();
client.ApplyTheme(css);
```

## Parameters

### rx.theme

| Parameter          | Documentation                                                                                      | Ivy                                                                 |
|--------------------|-----------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `has_background`   | Whether to apply the theme's background color to the theme node. `bool`.                           | Not supported (background is part of `ThemeColors.Background`)      |
| `appearance`       | The appearance of the theme: `"inherit"`, `"light"`, or `"dark"`. Defaults to `"light"`.           | `client.SetThemeMode(ThemeMode.Light / Dark / System)`              |
| `accent_color`     | Primary color used for buttons, typography, backgrounds. e.g. `"teal"`, `"red"`, `"tomato"`.       | `ThemeColors.Primary` / `ThemeColors.Accent`                        |
| `gray_color`       | Secondary color used for buttons, typography, backgrounds. e.g. `"gray"`, `"mauve"`.               | `ThemeColors.Muted` / `ThemeColors.MutedForeground`                 |
| `panel_background` | Whether panel backgrounds are translucent: `"solid"` or `"translucent"`.                           | Not supported                                                       |
| `radius`           | Border radius for all components: `"none"`, `"small"`, `"medium"`, `"large"`, `"full"`.            | Not supported                                                       |
| `scaling`          | Scale of all theme items: `"90%"`, `"95%"`, `"100%"`, `"105%"`, `"110%"`.                          | Not supported                                                       |

### rx.theme_panel

| Parameter          | Documentation                                                                                      | Ivy                                                                 |
|--------------------|-----------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `default_open`     | Whether the theme panel is open by default. `bool`.                                                | Not supported (no visual theme panel component)                     |
