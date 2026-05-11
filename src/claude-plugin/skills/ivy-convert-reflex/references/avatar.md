# Avatar

Displays a user's profile picture with an automatic fallback to initials or a placeholder when the image is unavailable.

## Reflex

```python
rx.avatar(
    src="https://example.com/profile.jpg",
    fallback="RU",
    size="3",
)
```

## Ivy

```csharp
new Avatar("Reflex User", "https://example.com/profile.jpg")
```

## Parameters

| Parameter      | Documentation                                          | Ivy                        |
|----------------|--------------------------------------------------------|----------------------------|
| src / Image    | URL of the avatar image                                | `Image` property           |
| fallback       | Text shown when image fails (e.g. initials)            | `Fallback` (constructor)   |
| size           | Size on a 1-9 scale                                    | `Width` / `Height`         |
| variant        | Visual style of fallback (`"solid"` or `"soft"`)       | Not supported              |
| color_scheme   | Color theme for the fallback text                      | Not supported              |
| high_contrast  | Increases contrast of the fallback text                | Not supported              |
| radius         | Border radius (`"none"`, `"small"`, ..., `"full"`)     | Not supported              |
| Visible        | Controls visibility                                    | `Visible` property         |
| Scale          | Scaling configuration                                  | `Scale` property           |
