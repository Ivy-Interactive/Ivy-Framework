---
searchHints:
  - wallpaper
  - background
  - chrome
  - ui
  - customization
---

# Wallpaper

<Ingress>
Configure a dedicated background *app* that appears when no other tabs are open.  Perfect for welcome screens, dashboards or branded imagery.
</Ingress>

The **Wallpaper** is just another Ivy application rendered full-screen by the Chrome host whenever the tab area is empty.  This keeps your UI visually engaging instead of showing an empty canvas.

## When is the Wallpaper shown?

* Only when using `DefaultSidebarChrome` with **tab navigation** (`UseTabs`).
* When the current tab list becomes empty – e.g. right after start-up or after the user closes every tab.
* It is **not** displayed when you switch Chrome to page navigation (`UsePages`).

## Enabling a Wallpaper

The wallpaper is selected through `ChromeSettings.WallpaperAppId`.  Two helper extensions make this convenient:

```csharp
// Explicit id
var chromeSettings = ChromeSettings.Default()
    .WallpaperAppId("welcome-screen");

// Or using a type – compile-time safety
chromeSettings = chromeSettings.WallpaperApp<WelcomeScreenApp>();
```

1. Implement a normal Ivy app (derive from `ViewBase`).
2. Register it like any other app (`server.AddApp<WelcomeScreenApp>()`).
3. Reference it in `ChromeSettings` with one of the helpers above.

## Full example

```csharp
public class WelcomeScreenApp : ViewBase
{
    public override object? Build()
        => Layout.Center(
            new Image("/img/brand-logo.svg").AltText("My Brand"),
            Text.Heading("Welcome to My System")
        );
}

var server = new Server();
server.AddAppsFromAssembly();

var chromeSettings = ChromeSettings.Default()
    .WallpaperApp<WelcomeScreenApp>()
    .UseTabs();

server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
await server.RunAsync();
```

## Tips

* The wallpaper **receives a `ConnectionId`** just like normal apps, so you can make it interactive (e.g. show login, stats, or tips).
* Keep resource size small so initial load feels snappy.
* Combine with **Theming** to adapt imagery to light / dark modes.

<Callout Type="tip">
Need something simpler?  You can also display static imagery by creating an app that only returns an `Image` widget.
</Callout>
