# Image

Displays images from URLs, local assets, or embedded data. Supports responsive sizing and styling.

## Reflex

```python
rx.image(
    src="/logo.jpg",
    width="100px",
    height="auto",
    border_radius="15px 50px",
    border="5px solid #555",
)
```

## Ivy

```csharp
new Image("https://example.com/image.jpg")
{
    Width = 100,
    Height = Size.Auto,
}
```

## Parameters

| Parameter        | Documentation                                          | Ivy           |
|------------------|--------------------------------------------------------|---------------|
| `src`            | Image source (URL, PIL image, or local asset)          | `Src`         |
| `alt`            | Alternative text for accessibility                     | Not supported |
| `width`          | Horizontal dimension of the image                      | `Width`       |
| `height`         | Vertical dimension of the image                        | `Height`      |
| `loading`        | Loading strategy (`"eager"` or `"lazy"`)               | Not supported |
| `cross_origin`   | CORS setting (`"anonymous"` or `"use-credentials"`)    | Not supported |
| `decoding`       | Image decoding method (`"async"` or `"auto"`)          | Not supported |
| `referrer_policy`| Referrer policy for the image request                  | Not supported |
| `sizes`          | Responsive image sizes                                 | Not supported |
| `src_set`        | Set of image sources for responsive images             | Not supported |
| `use_map`        | Image map reference                                    | Not supported |
| N/A              | N/A                                                    | `Scale`       |
| N/A              | N/A                                                    | `Visible`     |
