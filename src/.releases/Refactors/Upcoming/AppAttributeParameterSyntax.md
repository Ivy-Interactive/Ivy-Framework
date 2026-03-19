---
description: Rewrite AppAttribute PascalCase properties to lowercase parameter syntax.
---
# AppAttribute Parameter Syntax Refactor

## Summary
The `AppAttribute` uses named parameters with colon syntax (e.g., `icon:`, `title:`), not property syntax with equals (e.g., `Icon =`, `Title =`). This refactoring rule automatically corrects the common hallucination of using PascalCase property syntax instead of the correct lowercase parameter syntax.

## Changes
This refactoring rule handles the following transformations:

### Corrects PascalCase to Lowercase Parameter Syntax
- `[App(Icon = Icons.MessageSquare)]` → `[App(icon: Icons.MessageSquare)]`
- `[App(Id = "studio", Title = "Studio")]` → `[App(id: "studio", title: "studio")]`
- `[App(Icon = Icons.X, Path = ["Tests"])]` → `[App(icon: Icons.X, path: ["Tests"])]`

### Removes Invalid Hallucinated Parameters
- `[App(Group = "Apps")]` → `[App()]` (Group parameter doesn't exist)
- `[App(Chrome = "full")]` → `[App()]` (Chrome parameter doesn't exist)
- `[App(icon: Icons.X, Group = "Apps")]` → `[App(icon: Icons.X)]` (removes only invalid parameters)

### Preserves Correct Syntax
- `[App(icon: Icons.House)]` → No change (already correct)
- `[App(icon: Icons.X, title: "Test")]` → No change (already correct)
- `[App()]` → No change (empty attribute)

## Valid Parameters
The refactoring rule recognizes these valid `AppAttribute` parameters:
- `id`, `title`, `icon`, `description`, `path`, `isVisible`, `order`, `groupExpanded`, `documentSource`, `searchHints`

**Note**: The `path` parameter will be renamed to `group` in a future release (see [Issue #2587](https://github.com/Ivy-Interactive/Ivy-Framework/issues/2587)).

## Refactoring Instructions
This refactoring is applied automatically by the Ivy Agent when writing C# files. No manual intervention is required. If you encounter the compiler error `CS0655: '...' is not a valid named attribute argument`, this rule will fix it on the next agent operation.
