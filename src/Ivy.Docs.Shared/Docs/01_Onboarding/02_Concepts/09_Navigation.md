---
searchHints:
  - navigation
  - routing
  - usenavigation
  - navigate
  - apps
  - deeplink
  - urls
  - appshell
  - navigation-args
  - route
  - hyperlink
  - link
  - anchor
---

# Navigation

<Ingress>
The UseNavigation hook provides a powerful way to navigate between [apps](./10_Apps.md) and [external URLs](./13_Clients.md) in Ivy [applications](./10_Apps.md), enabling seamless user experiences and [deep linking](#navigation-with-arguments) capabilities.
</Ingress>

## Overview

Navigation in Ivy is handled through the `UseNavigation()` hook, which returns an `INavigator` interface. This hook enables:

- **App-to-App Navigation** - Navigate between different Ivy [apps](./10_Apps.md) within your application
- **External URL Navigation** - Open external URLs and resources
- **Deep Linking** - Navigate to specific apps with deep linking parameters and [arguments](../../03_Hooks/02_Core/13_UseArgs.md)
- **Type-Safe Navigation** - Navigate using strongly-typed app classes

The navigation system is built on top of Ivy's [signal system](../../03_Hooks/02_Core/10_UseSignal.md) and integrates seamlessly with the [AppShell](./11_AppShell.md) framework for managing app lifecycle and routing.

## How UseNavigation Works

```mermaid
flowchart TD
    A[View Component] --> B[UseNavigation Hook]
    B --> C[INavigator Interface]
    C --> D{Navigation Type?}
    D -->|Type-Safe| E[Navigate by Type]
    D -->|URI-Based| F[Navigate by URI]
    D -->|External| G[Open External URL]
    E --> H[AppShell System]
    F --> H
    G --> I[Browser/External Handler]
    H --> J[Target App]
```

## Basic Usage

### Getting the Navigator

Get the navigator in any [view](./02_Views.md) and use it with [Button](../../02_Widgets/03_Common/01_Button.md) or other widgets:

```csharp
[App(icon: Icons.Navigation)]
public class MyNavigationApp : ViewBase
{
    public override object? Build()
    {
        // Get the navigator instance
        var navigator = UseNavigation();
        
        return new Button("Navigate to Another App")
            .OnClick(() => navigator.Navigate(typeof(AnotherApp)));
    }
}
```

### Navigation Methods

The `INavigator` interface provides two main navigation methods:

```csharp
public interface INavigator
{
    // Navigate using app type (type-safe)
    void Navigate(Type type, object? appArgs = null);
    
    // Navigate using URI string (flexible)
    void Navigate(string uri, object? appArgs = null);
}
```

## Navigation Patterns

### Type-Safe Navigation

Navigate to [apps](./10_Apps.md) using their class types for compile-time safety:

```csharp
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        
        return Layout.Vertical(
            new Button("Go to User Profile")
                .OnClick(() => navigator.Navigate(typeof(UserProfileApp))),
                
            new Button("Open Settings")
                .OnClick(() => navigator.Navigate(typeof(SettingsApp))),
                
            new Button("View Reports")
                .OnClick(() => navigator.Navigate(typeof(ReportsApp)))
        );
    }
}
```

### Navigation with Arguments

Pass data to target apps using strongly-typed arguments. Receive them in the target app with [UseArgs](../../03_Hooks/02_Core/13_UseArgs.md):

```csharp
public record UserProfileArgs(int UserId, string Tab = "overview");

// Navigate with arguments
navigator.Navigate(typeof(UserProfileApp), new UserProfileArgs(123, "details"));

// Receive arguments in target app
public class UserProfileApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<UserProfileArgs>();
        return Text.Heading($"User Profile: {args?.UserId}");
    }
}
```

### URI-Based Navigation

Use URI strings for dynamic navigation scenarios:

```csharp
// Navigate using URI strings
navigator.Navigate("app://dashboard");
navigator.Navigate("app://users");
navigator.Navigate("app://settings");

// Dynamic navigation
var appUri = $"app://{selectedAppName}";
navigator.Navigate(appUri);
```

### External URL Navigation

Open external websites and resources:

```csharp
// Open external URLs
navigator.Navigate("https://docs.ivy-framework.com");
navigator.Navigate("https://github.com/ivy-framework/ivy");
navigator.Navigate("mailto:support@example.com");
```

## Navigation Helpers

Create reusable navigation patterns:

```csharp
public static class NavigationHelpers
{
    public static Action<string> UseLinks(this IView view)
    {
        var navigator = view.UseNavigation();
        return uri => navigator.Navigate(uri);
    }
    
    public static Action UseBackNavigation(this IView view, string defaultApp = "app://dashboard")
    {
        var navigator = view.UseNavigation();
        return () => navigator.Navigate(defaultApp);
    }
}

// Usage
var navigateToLink = UseLinks();
var goBack = UseBackNavigation();
```

### Integration with AppShell Settings

Navigation behavior can be configured through [AppShell](./11_AppShell.md) settings in your [Program](./01_Program.md):

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        IvyApp.Run(args, app =>
        {
            app.UseAppShell(AppShellSettings.Default()
                .UseTabs(preventDuplicates: true) // Prevent duplicate tabs
                .DefaultApp<DashboardApp>()       // Set default app
            );
        });
    }
}
```

### Navigation Modes

- **Tabs Mode**: Each navigation creates a new tab (default)
- **Pages Mode**: Navigation replaces the current view
- **Prevent Duplicates**: Avoid opening multiple tabs for the same app

## Navigation Beacons

Navigation Beacons enable apps to advertise their ability to handle specific entity types, allowing dynamic discovery and type-safe contextual navigation without hard-coding app IDs.

### Registering a Beacon

Apps register beacons using the `[NavigationBeacon]` attribute and a static factory method:

```csharp
public class Product { public int Id { get; set; } }

[App(icon: Icons.Package)]
[NavigationBeacon(typeof(Product), nameof(GetProductBeacon))]
public class ProductDetailsApp : ViewBase
{
    public static NavigationBeacon<Product> GetProductBeacon() => new(
        AppId: "product-details",
        ArgsBuilder: product => new { ProductId = product.Id }
    );

    public override object? Build()
    {
        var args = UseArgs<dynamic>();
        return Text.Heading($"Product #{args?.ProductId}");
    }
}
```

### Using Beacons for Navigation

Other apps can discover and navigate using beacons without knowing the target app's ID:

```csharp
public class ProductListApp : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        var productBeacon = UseNavigationBeacon<Product>();

        var products = new[] {
            new Product { Id = 1 },
            new Product { Id = 2 }
        };

        return Layout.Vertical(
            products.Select(product =>
                new Button($"View Product {product.Id}")
                    .OnClick(() => {
                        if (productBeacon != null)
                            navigator.Navigate(productBeacon, product);
                    })
            )
        );
    }
}
```

### Checking Beacon Availability

Use `UseNavigationBeacon<T>()` to check if a handler exists:

```csharp
var productBeacon = UseNavigationBeacon<Product>();

if (productBeacon != null)
{
    // Product detail app is available
    navigator.Navigate(productBeacon, product);
}
else
{
    // No app handles Product entities
    UseToast().Show("Product details not available", ToastType.Warning);
}
```

### Beacon Best Practices

- **One beacon per entity type**: Only one app can register a beacon for each entity type
- **Use meaningful entity types**: Define dedicated entity classes rather than reusing primitives
- **Handle missing beacons gracefully**: Always check if beacon exists before navigating
- **Keep ArgsBuilder simple**: Return anonymous objects with the minimum data needed for navigation

## Best Practices and Common Patterns

### Type-Safe Navigation

Prefer type-safe navigation over URI strings when possible:

```csharp
// Preferred: Type-safe navigation
navigator.Navigate(typeof(UserProfileApp), new UserProfileArgs(userId));

// Avoid: String-based navigation when type is known
navigator.Navigate($"app://user-profile?userId={userId}");
```

### Master-Detail Navigation

Navigate from list views to detail views using a [Table](../../02_Widgets/03_Common/08_Table.md):

```csharp
return new Table<Item>(items)
    .Column("Name", i => i.Name)
    .OnRowClick(item => 
        navigator.Navigate(typeof(ItemDetailApp), new ItemDetailArgs(item.Id))
    );
```

### Conditional Navigation

Navigate based on user permissions or [state](../../03_Hooks/02_Core/03_UseState.md):

```csharp
var handleNavigation = UseMemo(() => (Action)(() =>
{
    if (user.HasRole("Admin"))
        navigator.Navigate(typeof(AdminPanelApp));
    else
        navigator.Navigate(typeof(UnauthorizedApp));
}), user);
```

### Memoized Navigation Callbacks

Use [UseMemo](../../03_Hooks/02_Core/05_UseMemo.md) to memoize navigation callbacks:

```csharp
var navigateToUser = UseMemo(() => (Action<int>)((int userId) =>
{
    navigator.Navigate(typeof(UserProfileApp), new UserProfileArgs(userId));
}), navigator);
```

## Troubleshooting

### App Not Found Error

Ensure your app has the [App](./10_Apps.md) attribute:

```csharp
[App(icon: Icons.LayoutDashboard)]
public class MyApp : ViewBase { }
```

### Navigation Arguments Not Received

Ensure argument types match exactly between source and target apps:

```csharp
// Source: navigator.Navigate(typeof(TargetApp), new MyArgs("value"));
// Target: var args = UseArgs<MyArgs>(); // Same type
```

### External URLs Not Opening

Include the protocol in external URLs:

```csharp
navigator.Navigate("https://example.com"); // Correct
navigator.Navigate("example.com"); // Incorrect - treated as app URI
```

## Performance Considerations

- **Memoize Navigation Callbacks**: Use [UseMemo](../../03_Hooks/02_Core/05_UseMemo.md) to memoize navigation handlers
- **Lazy App Loading**: Apps are loaded on-demand when navigated to
- **State Cleanup**: Navigation automatically handles cleanup of previous app [state](../../03_Hooks/02_Core/03_UseState.md)
- **Memory Management**: The [AppShell](./11_AppShell.md) system manages app lifecycle and memory usage

## UseNavigation

The `UseNavigation` hook enables programmatic navigation:

- **Type-Safe Navigation** - Navigate to [apps](./10_Apps.md) using strongly-typed app classes
- **URI-Based Navigation** - Navigate using URI strings for dynamic scenarios
- **Navigation Arguments** - Pass data to target apps during navigation
- **External URL Navigation** - Open external websites and resources

### Basic Usage

```csharp
var navigator = UseNavigation();

// Navigate by URI
navigator.Navigate("app://hooks/core/usestate");

// Navigate by type
navigator.Navigate(typeof(MyApp));

// Navigate with arguments
navigator.Navigate(typeof(MyApp), new MyArgs(123));
```

### How Navigation Works

```mermaid
flowchart LR
    A[UseNavigation] --> B[INavigator]
    B --> C{Type?}
    C -->|Type-Safe| D[Navigate by Type]
    C -->|URI| E[Navigate by URI]
    C -->|External| F[Open URL]
    D --> G[Target App]
    E --> G
```

### Common Patterns

#### Navigation with Arguments

Pass data to target apps using strongly-typed arguments. Receive them with [UseArgs](../../03_Hooks/02_Core/13_UseArgs.md) in the target app:

```csharp
public record UserArgs(int UserId, string Tab = "overview");

// Navigate with arguments
var navigator = UseNavigation();
navigator.Navigate(typeof(TargetApp), new UserArgs(123, "settings"));

// Receive in target app
var args = UseArgs<UserArgs>();
```

#### External URL Navigation

Open external websites and resources:

```csharp
var navigator = UseNavigation();

navigator.Navigate("https://docs.ivy.app");
navigator.Navigate("mailto:support@example.com");
``` 

### Troubleshooting

**App Not Found**: Ensure your app has the [App](./10_Apps.md) attribute:

```csharp
[App(icon: Icons.LayoutDashboard)]
public class MyApp : ViewBase { }
```

**Arguments Not Received**: Ensure argument types match exactly between source and target:

```csharp
// Source: navigator.Navigate(typeof(TargetApp), new MyArgs("value"));
// Target: var args = UseArgs<MyArgs>(); // Same type
```

### Best Practices

- **Prefer type-safe navigation** - Use `Navigate(typeof(MyApp))` when target is known at compile time
- **Use records for arguments** - Pass data with strongly-typed argument objects
- **Include protocol for external URLs** - Always use `https://` or `mailto:` for external links
- **Ensure apps have [App](./10_Apps.md) attribute** - Target apps must be decorated with `[App]`

## Faq

### How do I create a hyperlink or link component?

Ivy does not have a dedicated Link widget. Use `Button` with `UseNavigation()` for internal navigation, or open external URLs:

```csharp
var nav = UseNavigation();

// Internal link
new Button("Go to Settings").OnClick(() => nav.Navigate(typeof(SettingsApp)));

// External link
new Button("Visit Docs").OnClick(() => nav.Navigate("https://docs.ivy.app"));
```

### How do I navigate to a different page?

Use the `UseNavigation()` hook:

```csharp
var nav = UseNavigation();
nav.Navigate(typeof(TargetApp));          // type-safe
nav.Navigate("app://path/to/app");        // URI-based
nav.Navigate("https://example.com");      // external URL
```

## See Also

- [AppShell](./11_AppShell.md)
- [Apps](./10_Apps.md)
- [UseArgs](../../03_Hooks/02_Core/13_UseArgs.md)
- [Views](./02_Views.md)
- [Signals](../../03_Hooks/02_Core/10_UseSignal.md)
- [State Management](../../03_Hooks/02_Core/03_UseState.md)
