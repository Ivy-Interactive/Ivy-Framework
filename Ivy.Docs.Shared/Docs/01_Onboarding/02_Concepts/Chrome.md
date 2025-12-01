---
searchHints:
  - chrome
  - sidebar
  - header
  - footer
  - navigation
  - tabs
  - pages
  - wallpaper
  - background
  - transformer
  - menu
---

# Chrome Configuration

<Ingress>
Configure the application chrome (sidebar, header, footer) using ChromeSettings to customize navigation, branding, and layout behavior.
</Ingress>

You can add custom elements to both the header and footer sections of the sidebar using `ChromeSettings`:

```csharp
var chromeSettings = new ChromeSettings()
    .Header(
        Layout.Vertical().Gap(2)
        | new IvyLogo()
        | Text.Lead("Enterprise Management System")
        | Text.Muted("Comprehensive business application suite")
    )
    .Footer(
        Layout.Vertical().Gap(2)
        | new Button("Support")
            .HandleClick(_ => { })
        | Text.Small("Enterprise Application Framework")
    )
    .DefaultApp<MyApp>()
    .UseTabs(preventDuplicates: true);

server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
```

## ChromeSettings Options

- **DefaultAppId(string? appId)** - Sets the default app to load by ID.

- **DefaultApp<T>()** - Sets the default app using a type (recommended for compile-time safety).

- **UseTabs(bool preventDuplicates)** - Enables tab navigation. When `preventDuplicates` is `true`, prevents duplicate tabs.

- **UsePages()** - Switches to page navigation (replaces content instead of opening tabs).

- **UseFooterMenuItemsTransformer(`Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>` transformer)** - Provides a way to dynamically transform the footer menu items. Useful for adding, removing, or re-ordering links based on runtime context such as user roles or navigation state.

- **WallpaperAppId(string? appId)** / **WallpaperApp<T>()** - Sets a dedicated *wallpaper* app that is shown whenever the tab list is empty. Handy for welcome screens or branded backgrounds.

<Callout Type="tip">
Use `server.UseDefaultApp(typeof(AppName))` instead of `UseChrome()` for single-purpose applications, embedded views, or minimal interfaces where sidebar navigation isn't needed.
</Callout>

## Wallpaper

Configure a dedicated background *app* that appears when no other tabs are open. Perfect for welcome screens, dashboards or branded imagery.

The **Wallpaper** is just another Ivy application rendered full-screen by the Chrome host whenever the tab area is empty. This keeps your UI visually engaging instead of showing an empty canvas.

### Configuration

The wallpaper is selected through `ChromeSettings.WallpaperAppId`. Two helper extensions make this convenient:

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

### Full Example

```csharp
public class WelcomeScreenApp : ViewBase
{
    public override object? Build()
        => Layout.Center(
            new Image("/ivy/img/brand-logo.svg").AltText("My Brand"),
            Text.H1("Welcome to My System")
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

## Footer Transformer

Dynamically customize the list of links shown at the very bottom of the sidebar by providing a transformation function with `UseFooterMenuItemsTransformer`.

`ChromeSettings.UseFooterMenuItemsTransformer` accepts a delegate with the following signature:

```csharp
Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>
```

- **items** – the menu items produced by Ivy (from discovered apps).
- **navigator** – helper you can use to build `MenuItem` actions that navigate to a URI or app.
- **return value** – the new collection that will be rendered. You can re-order, filter, or append items freely.

### Usage Example

```csharp
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        // Convert to list for easier manipulation
        var list = items.ToList();

        // Append a static link at the end
        list.Add(new MenuItem("Logout", _ => navigator.Navigate("app://logout"), Icons.Logout));

        // Move "Settings" to the top of the footer
        var settings = list.FirstOrDefault(i => i.Id == "app://settings");
        if (settings != null)
        {
            list.Remove(settings);
            list.Insert(0, settings);
        }

        return list;
    });
```

You can leverage a footer-menu items transformer to conditionally show or hide links, inject additional ones like "Docs", "Logout", or "Change theme", and rearrange or group items without having to update each individual app.

### Role-Based Filtering

```csharp
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        var user = AuthContext.CurrentUser;

        // Hide admin-only links for non-admins
        var filtered = items.Where(i =>
            !i.Tags.Contains("admin") || user?.IsInRole("admin") == true);

        return filtered;
    });
```

In this example we tag certain `MenuItem`s with the custom tag `admin` when generating them elsewhere. The transformer then checks the current user's roles (via your auth system) and removes admin-only links for non-admins.

For more information about SideBar, check its [documentation](../../02_Widgets/04_Layouts/SidebarLayout.md)
