---
searchHints:
  - footer
  - menu
  - transformer
  - chrome
  - sidebar
---

# Footer Menu Items Transformer

<Ingress>
Dynamically customize the list of links shown at the very bottom of the sidebar by providing a transformation function with `UseFooterMenuItemsTransformer`.
</Ingress>

`ChromeSettings.UseFooterMenuItemsTransformer` accepts a delegate with the following signature:

```csharp
Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>
```

- **items** – the menu items produced by Ivy (from discovered apps).  

- **navigator** – helper you can use to build `MenuItem` actions that navigate to a URI or app.  
- **return value** – the new collection that will be rendered. You can re-order, filter, or append items freely.

## Basic Usage

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

You can leverage a footer-menu items transformer to conditionally show or hide links, inject additional ones like “Docs”, “Logout”, or “Change theme”, and rearrange or group items without having to update each individual app.

## Role-Based Filtering

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

In this example we tag certain `MenuItem`s with the custom tag `admin` when generating them elsewhere. The transformer then checks the current user’s roles (via your auth system) and removes admin-only links for non-admins.
