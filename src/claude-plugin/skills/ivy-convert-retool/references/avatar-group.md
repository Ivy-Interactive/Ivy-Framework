# Avatar Group

Displays a group of avatars to represent users or entities. Shows profile images with automatic fallback to initials or placeholders. Automatically adjusts the number of visible avatars based on available width or a specified maximum, displaying a count badge for additional avatars that cannot be shown.

## Retool

```toolscript
avatarGroup1.images = [
  "https://example.com/user1.jpg",
  "https://example.com/user2.jpg",
  "https://example.com/user3.jpg"
];
avatarGroup1.fallbacks = ["Alice", "Bob", "Charlie"];
avatarGroup1.imageSize = 32;
avatarGroup1.maxItems = 3;
avatarGroup1.horizontalAlign = "left";
```

## Ivy

Ivy does not have a dedicated Avatar Group widget. Multiple avatars are composed using individual `Avatar` widgets inside layout containers such as `Layout.Horizontal()`.

```csharp
Layout.Horizontal()
    | new Avatar("Alice", "https://example.com/user1.jpg")
    | new Avatar("Bob", "https://example.com/user2.jpg")
    | new Avatar("Charlie", "https://example.com/user3.jpg")
```

## Parameters

| Parameter          | Documentation                                                        | Ivy                                                              |
|--------------------|----------------------------------------------------------------------|------------------------------------------------------------------|
| `images`           | Array of image URLs to display                                       | `Image` property on each `Avatar`                                |
| `fallbacks`        | Text displayed when images are unavailable (initials)                | `Fallback` property on each `Avatar`                             |
| `imageSize`        | Avatar dimensions: 16, 24, 32, 48, or 64px                          | `Width` / `Height` via `Size.Units()`                            |
| `maxItems`         | Maximum avatars shown before overflow count badge                    | Not supported                                                    |
| `horizontalAlign`  | Alignment: `left`, `center`, or `right`                              | Controlled by layout container                                   |
| `hidden`           | Controls component visibility                                        | `Visible` property on each `Avatar`                              |
| `isHiddenOnMobile` | Hide on mobile layout                                                | Not supported                                                    |
| `isHiddenOnDesktop`| Hide on desktop layout                                               | Not supported                                                    |
| `maintainSpaceWhenHidden` | Preserve layout space when hidden                             | Not supported                                                    |
| `margin`           | Outer spacing (`4px 8px` or `0`)                                     | Controlled by layout container                                   |
| `style`            | Custom styling options                                               | Not supported                                                    |
| `scrollIntoView()` | Method to scroll component into viewport                             | Not supported                                                    |
| `setHidden()`      | Method to toggle visibility                                          | `Visible` property                                               |
