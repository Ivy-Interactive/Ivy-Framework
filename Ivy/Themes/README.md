# Ivy Theme System

The Ivy theme system allows you to customize the visual appearance of your application by defining colors, fonts, and other style properties.

## Quick Start

### 1. Configure a theme in your server

```csharp
var server = new Server()
    .UseTheme(theme => 
    {
        theme.Name = "My Custom Theme";
        theme.Colors = new ThemeColors
        {
            Primary = "#0077BE",
            PrimaryForeground = "#FFFFFF",
            Secondary = "#5B9BD5",
            SecondaryForeground = "#FFFFFF",
            Background = "#F0F8FF",
            Foreground = "#1A1A1A",
            // ... other colors
        };
    });
```

### 2. Use predefined themes

```csharp
// Use the default theme
server.UseTheme(ThemeConfig.Default);

// Or create your own theme object
var myTheme = new ThemeConfig
{
    Name = "Ocean",
    Colors = new ThemeColors
    {
        Primary = "#0077BE",
        // ... other colors
    }
};
server.UseTheme(myTheme);
```

## Available Theme Properties

### Colors

The `ThemeColors` class supports the following color properties:

#### Primary Colors
- `Primary` - Main brand color
- `PrimaryForeground` - Text color on primary backgrounds
- `Secondary` - Supporting color
- `SecondaryForeground` - Text color on secondary backgrounds

#### Semantic Colors
- `Success` / `SuccessForeground` - Success states
- `Destructive` / `DestructiveForeground` - Error states
- `Warning` / `WarningForeground` - Warning states  
- `Info` / `InfoForeground` - Informational states

#### UI Element Colors
- `Background` / `Foreground` - Main page colors
- `Card` / `CardForeground` - Card component colors
- `Muted` / `MutedForeground` - Muted/disabled states
- `Accent` / `AccentForeground` - Accent/highlight colors
- `Border` - Border color
- `Input` - Input field background
- `Ring` - Focus ring color

### Other Properties

- `FontFamily` - Override the default font family
- `FontSize` - Override the base font size
- `BorderRadius` - Override the default border radius

## Theme Customizer App

The Ivy samples include a Theme Customizer app that allows you to:
- Preview different color combinations in real-time
- Export theme configurations as C# code or JSON
- Test themes with various UI components

Run the samples and navigate to UI > Theme Customizer to try it out.

## Dynamic Theme Changes

Themes can be changed dynamically using the Theme API:

```csharp
// In your controller or service
[HttpPost("api/theme/apply")]
public IActionResult ApplyTheme([FromBody] ThemeConfig theme)
{
    _themeService.SetTheme(theme);
    return Ok();
}
```

## CSS Variables

The theme system works by injecting CSS variables that override the default values in the frontend. All theme colors are available as CSS variables:

```css
:root {
    --primary: #0077BE;
    --primary-foreground: #FFFFFF;
    /* ... other variables */
}
```

You can use these variables in your custom CSS:

```css
.my-custom-element {
    background-color: var(--primary);
    color: var(--primary-foreground);
}
```
