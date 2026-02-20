# Button URL Default Target Changed to Same Tab - v1.2.15

## Summary

Buttons with `.Url()` now open in the **same tab** by default. Previously, URL buttons always opened in a new tab (`_blank`). Use the new `.OpenInNewTab()` extension method to restore the old behavior.

## What Changed

### Before (v1.2.14 and earlier)

```csharp
// Opened in a NEW tab by default
new Button("Visit Docs").Url("https://docs.example.com")
```

### After (v1.2.15+)

```csharp
// Opens in the SAME tab by default
new Button("Visit Docs").Url("https://docs.example.com")

// To open in a new tab, explicitly call .OpenInNewTab()
new Button("Visit Docs").Url("https://docs.example.com").OpenInNewTab()
```

## How to Find Affected Code

This is a **behavioral change**, not a compilation error. Search for buttons using `.Url()`:

```regex
\.Url\(
```

## How to Refactor

### If you want links to open in a new tab (old behavior)

Add `.OpenInNewTab()` to any Button that uses `.Url()` where new-tab behavior is desired:

**Before:**

```csharp
new Button("External Link", variant: ButtonVariant.Secondary)
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .Icon(Icons.ExternalLink, Align.Right)
```

**After:**

```csharp
new Button("External Link").Secondary()
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .OpenInNewTab()
    .Icon(Icons.ExternalLink, Align.Right)
```

### If same-tab navigation is fine

No changes needed — the new default behavior will apply automatically.

## Verification

Visually inspect buttons with URLs to ensure they open in the correct tab context.
