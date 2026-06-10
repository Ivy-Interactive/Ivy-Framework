# Ivy Framework Weekly Notes - Week of 2026-06-01

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Bug Fixes

### DataTable Header Slots Density Scaling and Spacing
We have improved the spacing and density scaling behavior of the `DataTable` header slots. Previously, spacing was manually defined inside the slot builders (e.g. `Layout.Horizontal().Gap(2)`).
With this update, the layout and spacing within `HeaderLeft` and `HeaderRight` slots are handled automatically by the DataTable component on the frontend using the table's `density` configuration. 

Developers should now use `new Fragment()` to return multiple widgets within the `HeaderLeft` and `HeaderRight` builders, allowing the framework to scale spacing consistently across small, medium, and large densities:

```csharp
// Before:
.HeaderLeft(_ => Layout.Horizontal().Gap(2)
    | new Button("Export", icon: Icons.Download).Small()
    | new Badge("Live").Color(Colors.Blue).Small())

// After:
.HeaderLeft(_ => new Fragment()
    | new Button("Export", icon: Icons.Download).Small()
    | new Badge("Live").Color(Colors.Blue).Small())
```
