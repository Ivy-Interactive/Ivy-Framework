# Ivy Framework Weekly Notes - Week of 2026-05-28

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### Nested Views Support in Details and Navigation Property Builders
We have enhanced the `DetailsBuilder` and `NavigationPropertyBuilder` to support nested properties that implement the `IView` interface. When binding complex types inside a `Details` widget, the builder will now check for `IView` assignability and recursively render the view instead of outputting the default string representation of the object.

```csharp
public class CustomerView : ViewBase
{
    public override object? Build()
    {
        return new Details<Customer>()
            .Bind(CustomerState); // Properties implementing IView will render nested
    }
}
```

### Exposing Badge Color API in Rustino.NET
Upgraded `Rustino.NET` to version `0.3.5` and exposed the badge color API. This allows developers using the desktop container to configure window badge colors directly via the exposed window configuration methods.

---

## Refactoring & API Changes

### Removed `IncludeMargin` from QRCode Widget
To ensure visual consistency and standard sizing, the QR code widget now always renders without margins. The `IncludeMargin` configuration property has been removed from the C# API:

```csharp
// Before:
var qr = new QRCode("https://ivy.app").IncludeMargin(false);

// After (margins are now excluded by default):
var qr = new QRCode("https://ivy.app");
```

### Staging Version Label Helper Extraction
Refactored and extracted the staging version label retrieval logic out of `DocsServer` and `SamplesServer` into a new internal `ServerVersionHelper` utility class. This centralizes the version/deploy timestamp configuration logic and improves assembly boundary visibility control.

---

## Bug Fixes

### xterm EventHandler Migration
Migrated terminal event properties to use standard `EventHandler<T>` delegates instead of raw `Func<>` signatures. This resolves namespace shadowing issues and enables C# fluent extension method chaining:

```csharp
// Event handlers can now be chained fluently:
var terminal = new Terminal()
    .OnData(data => HandleInput(data))
    .OnKey(e => ProcessKey(e));
```

### C# Object Normalization
Improved reliability of general C# object normalization routines during the serialization boundary step, resolving issues with type checking on complex models.
