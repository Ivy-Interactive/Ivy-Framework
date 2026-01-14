---
searchHints:
  - app
  - attribute
  - routing
  - icon
  - search
  - deeplink
  - metadata
---

# Apps & The `[App]` Attribute

<Ingress>
The `[App]` attribute is the cornerstone of defining applications within the Ivy Framework. It transforms a standard C# class into a discoverable, routable, and feature-rich application component.
</Ingress>

## Overview

In Ivy, an "App" is a self-contained unit of functionality, typically represented by a class inheriting from [ViewBase](./02_Views.md). To make this class recognized by the framework as an App, you must decorate it with the `[App]` attribute.

This attribute provides essential metadata that the framework uses to:

1. **Generate Routes**: Automatically creates URL routes for navigation.
2. **Generate UI**: Populates [navigation](./14_Navigation.md) menus, search results, and window titles.
3. **Configure Behavior**: Controls visibility, ordering, and searchability.

## The `[App]` Attribute

The `[App]` attribute is found in the `Ivy.Apps` namespace.

```csharp
using Ivy.Apps;

[App(
    title: "Product Catalog", 
    icon: Icons.ShoppingBag, 
    searchHints: ["store", "items", "inventory"]
)]
public class ProductsApp : ViewBase
{
    // ...
}
```

### Attribute Parameters

| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| **`id`** | `string?` | `null` | A unique identifier for the app. If omitted, it is generated from the class name and namespace (see Route Generation below). |
| **`title`** | `string?` | `null` | The human-readable title of the app. Used in window titles and navigation menus. Defaults to a readable version of the class name. |
| **`icon`** | `Icons` | `Icons.None` | The [icon](../../02_Widgets/01_Primitives/02_Icon.md) representing the app in the navigation bar and search results. Uses the `Ivy.Shared.Icons` enum. |
| **`description`** | `string?` | `null` | A brief description of the app's purpose. May be shown in tooltips or app listings. |
| **`path`** | `string[]?` | `null` | Explicitly defines the navigation path. Overrides automatic generation from the namespace. |
| **`isVisible`** | `bool` | `true` | Controls whether the app appears in automatically generated navigation menus. Set to `false` for hidden apps or internal tools. |
| **`order`** | `int` | `0` | Controls the sorting order of the app within its group in the navigation menu. Lower numbers appear first. |
| **`groupExpanded`** | `bool` | `false` | If `true`, the navigation group containing this app will be expanded by default. |
| **`documentSource`** | `string?` | `null` | Specifies a source for documentation content, if the app is a documentation viewer. |
| **`searchHints`** | `string[]?` | `null` | An array of keywords to improve discoverability in the global search. |

## Route Generation

One of the most powerful features of the `[App]` attribute is automatic route generation. Ivy uses a convention-based approach to turn your C# class structure into clean, friendly URLs.

The logic works as follows:

1. **Namespace Parsing**: The framework looks for the segment `Apps` in your namespace.
2. **Path Extension**: Anything *after* `Apps` in the namespace is treated as a folder path.
3. **Kebab-Case Conversion**: CamelCase names are converted to kebab-case (e.g., `MyApp` -> `my-app`).

### Examples

| Class Name | Full Namespace | Generated Route ID | URL |
| :--- | :--- | :--- | :--- |
| `DashboardApp` | `MyProject.Apps` | `dashboard-app` | `/dashboard-app` |
| `UserProfile` | `MyProject.Apps.Settings` | `settings/user-profile` | `/settings/user-profile` |
| `AuditLog` | `MyProject.Apps.Admin.Logs` | `admin/logs/audit-log` | `/admin/logs/audit-log` |

### Customizing Routes

You can override the automatic generation using the `id` or `path` parameters, though sticking to conventions is recommended for consistency.

## Best Practices

* **Suffix with `App`**: It's common convention to name your app classes ending with `App` (e.g., `ProductsApp`), though the framework will automatically make the title readable (e.g., "Products").
* **Use `searchHints`**: Add synonyms for your app's functionality to make it easier for users to find via the [Command Palette](./14_Navigation.md) (Cmd/Ctrl+K).
* **Organize with Namespaces**: Use namespaces to group related apps. This automatically creates a structured hierarchy in your navigation menu.
