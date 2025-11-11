---
searchHints:
  - chrome
  - sidebar
  - header
  - footer
  - navigation
  - tabs
  - pages
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

- **UseFooterMenuItemsTransformer(`Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>` transformer)** - Provides a way to dynamically transform the footer menu items. Useful for adding, removing, or re-ordering links based on runtime context such as user roles or navigation state. See [Footer Transformer](./04_FooterTransformer.md).

- **WallpaperAppId(string? appId)** / **WallpaperApp<T>()** - Sets a dedicated *wallpaper* app that is shown whenever the tab list is empty. Handy for welcome screens or branded backgrounds. See [Wallpaper](./03_Wallpaper.md).

<Callout Type="tip">
Use `server.UseDefaultApp(typeof(AppName))` instead of `UseChrome()` for single-purpose applications, embedded views, or minimal interfaces where sidebar navigation isn't needed.
</Callout>

For more information about SideBar, check its [documentation](../../../02_Widgets/04_Layouts/SidebarLayout.md)

